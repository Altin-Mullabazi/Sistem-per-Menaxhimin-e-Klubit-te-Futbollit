using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    /// <summary>
    /// Stadium service interface defining CRUD operations
    /// </summary>
    public interface IStadiumService
    {
        // 1. Get paginated stadiums with filters and search
        Task<(IEnumerable<StadiumDto> stadiums, int totalCount)> GetStadiumsWithPaginationAsync(
            int page = 1,
            int pageSize = 10,
            string? searchName = null,
            string? city = null);

        // 2. Get stadium by ID with detailed info
        Task<StadiumDetailDto?> GetStadiumByIdAsync(int id);

        // 3. Create stadium
        Task<StadiumDto> CreateStadiumAsync(CreateStadiumDto createStadiumDto);

        // 4. Update stadium
        Task<StadiumDto?> UpdateStadiumAsync(int id, UpdateStadiumDto updateStadiumDto);

        // 5. Delete stadium (with validation - can't delete if has matches)
        Task<(bool success, string message)> DeleteStadiumAsync(int id);
    }

    public class StadiumService : IStadiumService
    {
        private readonly ApplicationDbContext _context;

        public StadiumService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get paginated stadiums with optional filters by city and search by name
        /// </summary>
        public async Task<(IEnumerable<StadiumDto> stadiums, int totalCount)> GetStadiumsWithPaginationAsync(
            int page = 1,
            int pageSize = 10,
            string? searchName = null,
            string? city = null)
        {
            var query = _context.Stadiums.AsQueryable();

            // Apply city filter
            if (!string.IsNullOrEmpty(city))
                query = query.Where(s => s.City.ToLower() == city.ToLower());

            // Apply search by name
            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(s => s.Name.ToLower().Contains(searchName.ToLower()));

            // Apply sorting by creation date
            query = query.OrderByDescending(s => s.CreatedAt);

            var totalCount = await query.CountAsync();

            var stadiums = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (stadiums.Select(MapToDto).ToList(), totalCount);
        }

        /// <summary>
        /// Get stadium by ID with detailed information including clubs and matches
        /// </summary>
        public async Task<StadiumDetailDto?> GetStadiumByIdAsync(int id)
        {
            var stadium = await _context.Stadiums.FirstOrDefaultAsync(s => s.Id == id);

            if (stadium == null)
                return null;

            // Get club that owns this stadium (if any)
            var clubs = new List<Club>();
            if (stadium.ClubId.HasValue)
            {
                var club = await _context.Clubs.FirstOrDefaultAsync(c => c.Id == stadium.ClubId);
                if (club != null)
                    clubs.Add(club);
            }

            // Get matches at this stadium
            var matches = await _context.Matches
                .Where(m => m.StadiumId == id)
                .ToListAsync();

            return MapToDetailDto(stadium, clubs, matches);
        }

        /// <summary>
        /// Create a new stadium with validation
        /// </summary>
        public async Task<StadiumDto> CreateStadiumAsync(CreateStadiumDto createStadiumDto)
        {
            // Validate capacity
            if (createStadiumDto.Capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0");

            var stadium = new Stadium
            {
                Name = createStadiumDto.Name,
                City = createStadiumDto.City,
                Capacity = createStadiumDto.Capacity,
                YearBuilt = createStadiumDto.YearBuilt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Stadiums.Add(stadium);
            await _context.SaveChangesAsync();

            return MapToDto(stadium);
        }

        /// <summary>
        /// Update existing stadium with validation
        /// </summary>
        public async Task<StadiumDto?> UpdateStadiumAsync(int id, UpdateStadiumDto updateStadiumDto)
        {
            var stadium = await _context.Stadiums.FindAsync(id);
            if (stadium == null)
                return null;

            // Validate capacity
            if (updateStadiumDto.Capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0");

            stadium.Name = updateStadiumDto.Name;
            stadium.City = updateStadiumDto.City;
            stadium.Capacity = updateStadiumDto.Capacity;
            stadium.YearBuilt = updateStadiumDto.YearBuilt;
            stadium.UpdatedAt = DateTime.UtcNow;

            _context.Stadiums.Update(stadium);
            await _context.SaveChangesAsync();

            return MapToDto(stadium);
        }

        /// <summary>
        /// Delete stadium with validation - cannot delete if stadium has matches scheduled
        /// </summary>
        public async Task<(bool success, string message)> DeleteStadiumAsync(int id)
        {
            var stadium = await _context.Stadiums.FindAsync(id);
            if (stadium == null)
                return (false, "Stadium not found");

            // Check if stadium has matches
            var matchCount = await _context.Matches.CountAsync(m => m.StadiumId == id);
            if (matchCount > 0)
                return (false, $"Cannot delete stadium with {matchCount} scheduled match(es)");

            _context.Stadiums.Remove(stadium);
            await _context.SaveChangesAsync();

            return (true, "Stadium deleted successfully");
        }

        private static StadiumDto MapToDto(Stadium stadium)
        {
            return new StadiumDto
            {
                Id = stadium.Id,
                Name = stadium.Name,
                City = stadium.City,
                Capacity = stadium.Capacity,
                YearBuilt = stadium.YearBuilt,
                CreatedAt = stadium.CreatedAt,
                UpdatedAt = stadium.UpdatedAt
            };
        }

        private static StadiumDetailDto MapToDetailDto(Stadium stadium, List<Club> clubs, List<Match> matches)
        {
            return new StadiumDetailDto
            {
                Id = stadium.Id,
                Name = stadium.Name,
                City = stadium.City,
                Capacity = stadium.Capacity,
                YearBuilt = stadium.YearBuilt,
                Clubs = clubs.Select(c => new ClubDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    City = c.City,
                    FoundedYear = c.FoundedYear
                }).ToList(),
                Matches = matches.Select(m => new StadiumMatchDto
                {
                    Id = m.Id,
                    HomeClubId = m.HomeClubId,
                    AwayClubId = m.AwayClubId,
                    MatchDate = m.MatchDate,
                    Time = m.Time,
                    HomeScore = m.HomeScore,
                    AwayScore = m.AwayScore,
                    Status = m.Status,
                    CompetitionType = m.CompetitionType,
                    StadiumId = m.StadiumId,
                    SeasonId = m.SeasonId
                }).ToList(),
                CreatedAt = stadium.CreatedAt,
                UpdatedAt = stadium.UpdatedAt
            };
        }
    }
}
