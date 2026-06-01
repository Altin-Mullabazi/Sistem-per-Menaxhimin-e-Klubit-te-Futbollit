using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public class SeasonService : ISeasonService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeasonService> _logger;

        public SeasonService(ApplicationDbContext context, ILogger<SeasonService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(IEnumerable<SeasonDto> Seasons, int TotalCount)> GetSeasonsAsync(int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            _logger.LogInformation("Retrieving seasons page {Page} pageSize {PageSize}", page, pageSize);

            var query = _context.Seasons
                .Include(s => s.Matches)
                .OrderByDescending(s => s.StartDate)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var seasons = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (seasons.Select(MapToDto), totalCount);
        }

        public async Task<SeasonDto?> GetSeasonByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving season with ID {Id}", id);

            var season = await _context.Seasons
                .Include(s => s.Matches)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            return season == null ? null : MapToDto(season);
        }

        public async Task<SeasonDto> CreateSeasonAsync(CreateSeasonDto createSeasonDto, string? userId = null)
        {
            _logger.LogInformation("Creating season {SeasonName}", createSeasonDto.Name);

            ValidateDateRange(createSeasonDto.StartDate, createSeasonDto.EndDate);
            await EnsureUniqueNameAsync(createSeasonDto.Name);
            await EnsureNoOverlappingDatesAsync(createSeasonDto.StartDate, createSeasonDto.EndDate);

            if (string.IsNullOrWhiteSpace(userId) && _context.Database.IsRelational())
            {
                userId = await _context.LegacyUsers.Select(user => user.Id).FirstOrDefaultAsync();
            }

            if (string.IsNullOrWhiteSpace(userId) && _context.Database.IsRelational())
            {
                throw new InvalidOperationException("Unable to resolve an owner user for the season");
            }

            var season = new Season
            {
                Name = createSeasonDto.Name.Trim(),
                StartDate = createSeasonDto.StartDate,
                EndDate = createSeasonDto.EndDate,
                Description = string.IsNullOrWhiteSpace(createSeasonDto.Description) ? null : createSeasonDto.Description.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId ?? string.Empty
            };

            _context.Seasons.Add(season);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Season created with ID {SeasonId}", season.Id);

            return MapToDto(season);
        }

        public async Task<SeasonDto?> UpdateSeasonAsync(int id, UpdateSeasonDto updateSeasonDto)
        {
            _logger.LogInformation("Updating season with ID {Id}", id);

            var season = await _context.Seasons.FindAsync(id);
            if (season == null)
            {
                _logger.LogWarning("Season with ID {Id} not found", id);
                return null;
            }

            ValidateDateRange(updateSeasonDto.StartDate, updateSeasonDto.EndDate);
            await EnsureUniqueNameAsync(updateSeasonDto.Name, id);
            await EnsureNoOverlappingDatesAsync(updateSeasonDto.StartDate, updateSeasonDto.EndDate, id);

            season.Name = updateSeasonDto.Name.Trim();
            season.StartDate = updateSeasonDto.StartDate;
            season.EndDate = updateSeasonDto.EndDate;
            season.Description = string.IsNullOrWhiteSpace(updateSeasonDto.Description) ? null : updateSeasonDto.Description.Trim();
            season.UpdatedAt = DateTime.UtcNow;

            _context.Seasons.Update(season);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Season with ID {Id} updated", id);

            return MapToDto(season);
        }

        public async Task<bool> DeleteSeasonAsync(int id)
        {
            _logger.LogInformation("Deleting season with ID {Id}", id);

            var season = await _context.Seasons.FindAsync(id);
            if (season == null)
            {
                _logger.LogWarning("Season with ID {Id} not found for deletion", id);
                return false;
            }

            _context.Seasons.Remove(season);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Season with ID {Id} deleted", id);
            return true;
        }

        private static void ValidateDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
                throw new InvalidOperationException("Start date must be before end date");
        }

        private async Task EnsureUniqueNameAsync(string name, int? excludeSeasonId = null)
        {
            var normalized = name.Trim();
            var hasDuplicate = await _context.Seasons.AnyAsync(s => s.Name == normalized && (!excludeSeasonId.HasValue || s.Id != excludeSeasonId.Value));
            if (hasDuplicate)
                throw new InvalidOperationException("Season name must be unique");
        }

        private async Task EnsureNoOverlappingDatesAsync(DateTime startDate, DateTime endDate, int? excludeSeasonId = null)
        {
            var hasOverlap = await _context.Seasons.AnyAsync(s => (!excludeSeasonId.HasValue || s.Id != excludeSeasonId.Value)
                && s.StartDate <= endDate
                && s.EndDate >= startDate);

            if (hasOverlap)
                throw new InvalidOperationException("Season date range overlaps with an existing season");
        }

        private static SeasonDto MapToDto(Season season)
        {
            return new SeasonDto
            {
                Id = season.Id,
                Name = season.Name,
                StartDate = season.StartDate,
                EndDate = season.EndDate,
                Description = season.Description,
                CreatedAt = season.CreatedAt,
                UpdatedAt = season.UpdatedAt
            };
        }
    }
}
