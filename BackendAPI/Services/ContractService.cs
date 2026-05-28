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
            var query = _context.Contracts
                .Include(c => c.Player)
                .Include(c => c.Club)
                .AsQueryable();

            // Apply filters
            if (parameters.PlayerId.HasValue)
            {
                query = query.Where(c => c.PlayerId == parameters.PlayerId.Value);
            }

            if (parameters.IsActive.HasValue)
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
            var expiryDate = DateTime.UtcNow.AddDays(days);

            var query = _context.Contracts
                .Include(c => c.Player)
                .Include(c => c.Club)
                .Where(c => c.Status == ContractStatus.Active && c.EndDate <= expiryDate)
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
            // ✅ TRANSACTIONAL: Ensure atomic operation
            // Use transaction to guarantee only ONE active contract per player
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Business logic: Ensure only one active contract per player
                if (createContractDto.IsActive)
                {
                    // Find and deactivate any existing active contract
                    var existingActiveContract = await _context.Contracts
                        .FirstOrDefaultAsync(c => c.PlayerId == createContractDto.PlayerId && 
                                                 c.Status == ContractStatus.Active);

                    if (existingActiveContract != null)
                    {
                        existingActiveContract.Status = ContractStatus.Expired;
                        existingActiveContract.UpdatedAt = DateTime.UtcNow;
                        _context.Contracts.Update(existingActiveContract);
                    }
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
                await transaction.CommitAsync();

                // Load navigation properties
                await _context.Entry(contract).Reference(c => c.Player).LoadAsync();
                await _context.Entry(contract).Reference(c => c.Club).LoadAsync();

                return MapToDto(contract);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ContractDto?> UpdateContractAsync(int id, UpdateContractDto updateContractDto)
        {
            // ✅ TRANSACTIONAL: Ensure atomic operation
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var contract = await _context.Contracts.FindAsync(id);
                if (contract == null)
                    return null;

                // Business logic: Ensure only one active contract per player
                if (updateContractDto.IsActive && contract.Status != ContractStatus.Active)
                {
                    // Need to deactivate any existing active contract for this player
                    var existingActiveContract = await _context.Contracts
                        .FirstOrDefaultAsync(c => c.PlayerId == contract.PlayerId && 
                                                 c.Id != id &&
                                                 c.Status == ContractStatus.Active);

                    if (existingActiveContract != null)
                    {
                        existingActiveContract.Status = ContractStatus.Expired;
                        existingActiveContract.UpdatedAt = DateTime.UtcNow;
                        _context.Contracts.Update(existingActiveContract);
                    }
                }

                contract.EndDate = updateContractDto.EndDate;
                contract.Salary = updateContractDto.Salary;
                contract.Position = updateContractDto.Position;
                contract.Status = updateContractDto.IsActive ? ContractStatus.Active : ContractStatus.Expired;
                contract.UpdatedAt = DateTime.UtcNow;

                _context.Contracts.Update(contract);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Load navigation properties
                await _context.Entry(contract).Reference(c => c.Player).LoadAsync();
                await _context.Entry(contract).Reference(c => c.Club).LoadAsync();

                return MapToDto(contract);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return false;

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            return true;
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