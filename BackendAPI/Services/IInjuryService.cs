using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public interface IInjuryService
    {
        Task<PaginatedInjuryResponse> GetInjuriesAsync(
            int page = 1,
            int pageSize = 10,
            int? playerId = null,
            string? status = null,
            string sortBy = "date"
        );
        
        Task<PaginatedInjuryResponse> GetActiveInjuriesAsync(int page = 1, int pageSize = 10);
        
        Task<InjuryDto?> GetInjuryByIdAsync(int id);
        
        Task<InjuryDto> CreateInjuryAsync(CreateInjuryDto createInjuryDto);
        
        Task<InjuryDto?> UpdateInjuryAsync(int id, UpdateInjuryDto updateInjuryDto);
        
        Task<bool> DeleteInjuryAsync(int id);
    }

    public class InjuryService : IInjuryService
    {
        private readonly ApplicationDbContext _context;

        public InjuryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedInjuryResponse> GetInjuriesAsync(
            int page = 1,
            int pageSize = 10,
            int? playerId = null,
            string? status = null,
            string sortBy = "date")
        {
            ValidatePagination(page, pageSize);

            var query = _context.Injuries.Include(i => i.Player).AsQueryable();

            // Apply filters
            if (playerId.HasValue)
                query = query.Where(i => i.PlayerId == playerId.Value);

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<InjuryStatus>(status, ignoreCase: true, out var injuryStatus))
                {
                    query = query.Where(i => i.Status == injuryStatus);
                }
                else
                {
                    throw new ArgumentException("Status must be Active, Recovering, or Recovered");
                }
            }

            // Apply sorting (default: newest first)
            query = sortBy.ToLower() switch
            {
                "date" => query.OrderByDescending(i => i.InjuryDate),
                "player" => query.OrderBy(i => i.Player.FirstName).ThenBy(i => i.Player.LastName),
                _ => query.OrderByDescending(i => i.InjuryDate)
            };

            // Get total count
            var totalItems = await query.CountAsync();

            // Apply pagination
            var skip = (page - 1) * pageSize;
            var injuries = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var dtos = injuries.Select(i => MapToDto(i)).ToList();

            return new PaginatedInjuryResponse
            {
                Data = dtos,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }

        public async Task<PaginatedInjuryResponse> GetActiveInjuriesAsync(int page = 1, int pageSize = 10)
        {
            ValidatePagination(page, pageSize);

            var query = _context.Injuries
                .Include(i => i.Player)
                .Where(i => i.Status != InjuryStatus.Recovered)
                .OrderByDescending(i => i.InjuryDate);

            var totalItems = await query.CountAsync();
            var injuries = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedInjuryResponse
            {
                Data = injuries.Select(MapToDto).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }

        private static void ValidatePagination(int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than 0.");

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");
        }

        public async Task<InjuryDto?> GetInjuryByIdAsync(int id)
        {
            var injury = await _context.Injuries
                .Include(i => i.Player)
                .FirstOrDefaultAsync(i => i.Id == id);

            return injury == null ? null : MapToDto(injury);
        }

        public async Task<InjuryDto> CreateInjuryAsync(CreateInjuryDto createInjuryDto)
        {
            // Validate player exists
            var player = await _context.Players.FindAsync(createInjuryDto.PlayerId);
            if (player == null)
                throw new ArgumentException($"Player with id {createInjuryDto.PlayerId} not found");

            // Validate InjuryDate is not in the future
            if (createInjuryDto.InjuryDate > DateTime.UtcNow.Date)
                throw new ArgumentException("InjuryDate cannot be in the future");

            var injury = new Injury
            {
                PlayerId = createInjuryDto.PlayerId,
                InjuryType = createInjuryDto.InjuryType,
                InjuryDate = createInjuryDto.InjuryDate,
                Notes = createInjuryDto.Notes,
                Status = InjuryStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Injuries.Add(injury);
            await _context.SaveChangesAsync();

            // Reload with Player to return full DTO
            await _context.Entry(injury).Reference(i => i.Player).LoadAsync();

            return MapToDto(injury);
        }

        public async Task<InjuryDto?> UpdateInjuryAsync(int id, UpdateInjuryDto updateInjuryDto)
        {
            var injury = await _context.Injuries
                .Include(i => i.Player)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (injury == null)
                return null;

            // Update RecoveryDate if provided
            if (updateInjuryDto.RecoveryDate.HasValue)
            {
                if (updateInjuryDto.RecoveryDate < injury.InjuryDate)
                    throw new ArgumentException("RecoveryDate cannot be before InjuryDate");

                injury.RecoveryDate = updateInjuryDto.RecoveryDate;
            }

            // Update Status if provided
            if (updateInjuryDto.Status.HasValue)
            {
                injury.Status = updateInjuryDto.Status.Value;
            }

            // Update Notes if provided
            if (updateInjuryDto.Notes != null)
                injury.Notes = updateInjuryDto.Notes;

            injury.UpdatedAt = DateTime.UtcNow;

            _context.Injuries.Update(injury);
            await _context.SaveChangesAsync();

            return MapToDto(injury);
        }

        public async Task<bool> DeleteInjuryAsync(int id)
        {
            var injury = await _context.Injuries.FindAsync(id);
            if (injury == null)
                return false;

            _context.Injuries.Remove(injury);
            await _context.SaveChangesAsync();

            return true;
        }

        private InjuryDto MapToDto(Injury injury)
        {
            return new InjuryDto
            {
                Id = injury.Id,
                PlayerId = injury.PlayerId,
                PlayerName = injury.Player != null 
                    ? $"{injury.Player.FirstName} {injury.Player.LastName}" 
                    : null,
                InjuryType = injury.InjuryType,
                InjuryDate = injury.InjuryDate,
                RecoveryDate = injury.RecoveryDate,
                Status = injury.Status.ToString(),
                Notes = injury.Notes,
                CreatedAt = injury.CreatedAt,
                UpdatedAt = injury.UpdatedAt
            };
        }
    }
}
