using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public class ContractService : IContractService
    {
        private readonly ApplicationDbContext _context;

        public ContractService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResponse<ContractDto>> GetContractsAsync(ContractQueryParameters parameters)
        {
            NormalizePagination(parameters);

            var query = _context.Contracts
                .Include(c => c.Player)
                .Include(c => c.Club)
                .AsQueryable();

            // Apply filters
            if (parameters.PlayerId.HasValue)
            {
                query = query.Where(c => c.PlayerId == parameters.PlayerId.Value);
            }

            if (parameters.Days.HasValue)
            {
                if (parameters.Days < 1 || parameters.Days > 365)
                    throw new ArgumentOutOfRangeException(nameof(parameters.Days), "Days must be between 1 and 365.");

                var today = DateTime.UtcNow.Date;
                var expiryDate = today.AddDays(parameters.Days.Value);

                query = query.Where(c => c.Status == ContractStatus.Active && c.EndDate >= today && c.EndDate <= expiryDate);
            }
            else if (parameters.IsActive.HasValue)
            {
                query = query.Where(c => c.Status == (parameters.IsActive.Value ? ContractStatus.Active : ContractStatus.Expired));
            }

            // Sort by StartDate descending
            query = query.OrderByDescending(c => c.StartDate);

            var totalCount = await query.CountAsync();
            var contracts = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PaginatedResponse<ContractDto>
            {
                Items = contracts.Select(MapToDto),
                TotalCount = totalCount,
                Page = parameters.Page,
                PageSize = parameters.PageSize
            };
        }

        public async Task<ContractDto?> GetContractByIdAsync(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Player)
                .Include(c => c.Club)
                .FirstOrDefaultAsync(c => c.Id == id);

            return contract == null ? null : MapToDto(contract);
        }

        public async Task<PaginatedResponse<ContractDto>> GetActiveContractsAsync(int page = 1, int pageSize = 10)
        {
            (page, pageSize) = NormalizePagination(page, pageSize);

            var query = _context.Contracts
                .Include(c => c.Player)
                .Include(c => c.Club)
                .Where(c => c.Status == ContractStatus.Active)
                .OrderByDescending(c => c.StartDate);

            var totalCount = await query.CountAsync();
            var contracts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<ContractDto>
            {
                Items = contracts.Select(MapToDto),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResponse<ContractDto>> GetExpiringContractsAsync(int days, int page = 1, int pageSize = 10)
        {
            (page, pageSize) = NormalizePagination(page, pageSize);
            if (days < 1 || days > 365)
                throw new ArgumentOutOfRangeException(nameof(days), "Days must be between 1 and 365.");

            var expiryDate = DateTime.UtcNow.AddDays(days);
            var today = DateTime.UtcNow.Date;

            var query = _context.Contracts
                .Include(c => c.Player)
                .Include(c => c.Club)
                .Where(c => c.Status == ContractStatus.Active && c.EndDate >= today && c.EndDate <= expiryDate)
                .OrderBy(c => c.EndDate);

            var totalCount = await query.CountAsync();
            var contracts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<ContractDto>
            {
                Items = contracts.Select(MapToDto),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ContractDto> CreateContractAsync(CreateContractDto createContractDto)
        {
            await ValidateCreateAsync(createContractDto);

            // ✅ TRANSACTIONAL: Ensure atomic operation
            // Use transaction to guarantee only ONE active contract per player
            await using var transaction = _context.Database.IsRelational()
                ? await _context.Database.BeginTransactionAsync()
                : null;

            try
            {
                // Business logic: Ensure only one active contract per player
                if (createContractDto.IsActive)
                {
                    await DeactivateActiveContractsAsync(createContractDto.PlayerId);
                }

                // Create new contract
                var contract = new Contract
                {
                    PlayerId = createContractDto.PlayerId,
                    ClubId = createContractDto.ClubId,
                    StartDate = createContractDto.StartDate,
                    EndDate = createContractDto.EndDate,
                    Salary = createContractDto.Salary,
                    Position = createContractDto.Position,
                    Status = createContractDto.IsActive ? ContractStatus.Active : ContractStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                // Load navigation properties
                await _context.Entry(contract).Reference(c => c.Player).LoadAsync();
                await _context.Entry(contract).Reference(c => c.Club).LoadAsync();

                return MapToDto(contract);
            }
            catch
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
        }

        public async Task<ContractDto?> UpdateContractAsync(int id, UpdateContractDto updateContractDto)
        {
            if (id < 1)
                throw new ArgumentOutOfRangeException(nameof(id), "Contract ID must be a positive number.");

            // ✅ TRANSACTIONAL: Ensure atomic operation
            await using var transaction = _context.Database.IsRelational()
                ? await _context.Database.BeginTransactionAsync()
                : null;

            try
            {
                var contract = await _context.Contracts.FindAsync(id);
                if (contract == null)
                    return null;

                ValidateUpdate(contract, updateContractDto);

                // Business logic: Ensure only one active contract per player
                if (updateContractDto.IsActive)
                {
                    await DeactivateActiveContractsAsync(contract.PlayerId, id);
                }

                contract.EndDate = updateContractDto.EndDate;
                contract.Salary = updateContractDto.Salary;
                contract.Position = updateContractDto.Position;
                contract.Status = updateContractDto.IsActive ? ContractStatus.Active : ContractStatus.Expired;
                contract.UpdatedAt = DateTime.UtcNow;

                _context.Contracts.Update(contract);
                await _context.SaveChangesAsync();
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                // Load navigation properties
                await _context.Entry(contract).Reference(c => c.Player).LoadAsync();
                await _context.Entry(contract).Reference(c => c.Club).LoadAsync();

                return MapToDto(contract);
            }
            catch
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            if (id < 1)
                throw new ArgumentOutOfRangeException(nameof(id), "Contract ID must be a positive number.");

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return false;

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            return true;
        }

        private static void NormalizePagination(ContractQueryParameters parameters)
        {
            (parameters.Page, parameters.PageSize) = NormalizePagination(parameters.Page, parameters.PageSize);
        }

        private static (int Page, int PageSize) NormalizePagination(int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than 0.");

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");

            return (page, pageSize);
        }

        private async Task ValidateCreateAsync(CreateContractDto createContractDto)
        {
            if (createContractDto.StartDate >= createContractDto.EndDate)
                throw new ArgumentException("Start date must be before end date.");

            if (createContractDto.Salary <= 0)
                throw new ArgumentException("Salary must be greater than 0.");

            if (!await _context.Players.AnyAsync(player => player.Id == createContractDto.PlayerId))
                throw new ArgumentException("Player not found.");

            if (!await _context.Clubs.AnyAsync(club => club.Id == createContractDto.ClubId))
                throw new ArgumentException("Club not found.");
        }

        private static void ValidateUpdate(Contract contract, UpdateContractDto updateContractDto)
        {
            if (contract.StartDate >= updateContractDto.EndDate)
                throw new ArgumentException("End date must be after start date.");

            if (updateContractDto.Salary <= 0)
                throw new ArgumentException("Salary must be greater than 0.");
        }

        private async Task DeactivateActiveContractsAsync(int playerId, int? exceptContractId = null)
        {
            var activeContracts = await _context.Contracts
                .Where(c => c.PlayerId == playerId &&
                            c.Status == ContractStatus.Active &&
                            (!exceptContractId.HasValue || c.Id != exceptContractId.Value))
                .ToListAsync();

            foreach (var activeContract in activeContracts)
            {
                activeContract.Status = ContractStatus.Expired;
                activeContract.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static ContractDto MapToDto(Contract contract)
        {
            return new ContractDto
            {
                Id = contract.Id,
                PlayerId = contract.PlayerId,
                ClubId = contract.ClubId,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Salary = contract.Salary,
                Position = contract.Position,
                IsActive = contract.IsActive,
                CreatedAt = contract.CreatedAt,
                UpdatedAt = contract.UpdatedAt,
                Player = contract.Player == null ? null : new PlayerDto
                {
                    Id = contract.Player.Id,
                    FirstName = contract.Player.FirstName,
                    LastName = contract.Player.LastName,
                    Position = contract.Player.Position,
                    JerseyNumber = contract.Player.JerseyNumber,
                    DateOfBirth = contract.Player.DateOfBirth,
                    Nationality = contract.Player.Nationality,
                    Height = contract.Player.Height,
                    Weight = contract.Player.Weight,
                    Status = contract.Player.Status?.ToString(),
                    MarketValue = contract.Player.MarketValue,
                    ClubId = contract.Player.ClubId,
                    CreatedAt = contract.Player.CreatedAt,
                    UpdatedAt = contract.Player.UpdatedAt
                },
                Club = contract.Club == null ? null : new ClubDto
                {
                    Id = contract.Club.Id,
                    Name = contract.Club.Name,
                    FoundedYear = contract.Club.FoundedYear,
                    City = contract.Club.City,
                    Budget = contract.Club.Budget,
                    CreatedAt = contract.Club.CreatedAt,
                    UpdatedAt = contract.Club.UpdatedAt
                }
            };
        }
    }
}
