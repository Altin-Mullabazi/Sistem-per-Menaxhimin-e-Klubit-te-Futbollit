using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public class ClubService : IClubService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClubService> _logger;

        public ClubService(ApplicationDbContext context, ILogger<ClubService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ EXTRACTED: DRY - Single method for name validation
        private async Task<bool> IsClubNameUniqueAsync(string name, int? excludeClubId = null)
        {
            var query = _context.Clubs
                .Where(c => c.Name == name);

            if (excludeClubId.HasValue)
                query = query.Where(c => c.Id != excludeClubId);

            return !await query.AnyAsync();
        }

        // ✅ ADDED: Validate founding year
        private void ValidateFoundedYear(int year)
        {
            var currentYear = DateTime.Now.Year;
            if (year < 1800 || year > currentYear)
            {
                _logger.LogWarning($"Invalid founded year: {year}");
                throw new InvalidOperationException(
                    $"Founded year must be between 1800 and {currentYear}");
            }
        }

        // ✅ ADDED: Validate budget
        private void ValidateBudget(decimal? budget)
        {
            if (budget.HasValue && budget < 0)
            {
                _logger.LogWarning($"Negative budget attempted: {budget}");
                throw new InvalidOperationException("Budget cannot be negative");
            }
        }

        // ✅ IMPROVED: Pagination validation in service
        private void ValidatePaginationParams(ref int page, ref int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;
        }

        // ✅ ADDED: Sort validation - reject invalid sort options
        private string ValidateSortBy(string sortBy)
        {
            var validOptions = new[] { "name", "city", "founded" };
            var normalized = sortBy?.ToLower().Trim() ?? "name";

            if (!validOptions.Contains(normalized))
            {
                _logger.LogWarning($"Invalid sort option: {sortBy}");
                throw new InvalidOperationException(
                    $"Invalid sort option. Valid options: {string.Join(", ", validOptions)}");
            }

            return normalized;
        }

        public async Task<ClubListResponseDto> GetAllClubsAsync(
            int page = 1, int pageSize = 10, string? search = null, 
            string? city = null, string sortBy = "name")
        {
            // ✅ Validate and sanitize inputs
            ValidatePaginationParams(ref page, ref pageSize);
            search = search?.Trim() ?? string.Empty;
            city = city?.Trim() ?? string.Empty;
            sortBy = ValidateSortBy(sortBy);

            _logger.LogInformation(
                $"Retrieving clubs: page={page}, pageSize={pageSize}, " +
                $"search={search}, city={city}, sortBy={sortBy}");

            var query = _context.Clubs.AsQueryable();

            // ✅ Search by name (SQL Server is case-insensitive by default)
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search));
                _logger.LogInformation($"Applying search filter: {search}");
            }

            // ✅ Filter by city (SQL Server is case-insensitive by default)
            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(c => c.City == city);
                _logger.LogInformation($"Applying city filter: {city}");
            }

            // Count total before pagination
            var totalCount = await query.CountAsync();

            // ✅ Sort (validated above, so safe to use switch)
            query = sortBy switch
            {
                "name" => query.OrderBy(c => c.Name),
                "city" => query.OrderBy(c => c.City),
                "founded" => query.OrderBy(c => c.FoundedYear),
                _ => query.OrderBy(c => c.Name)
            };

            // ✅ Include Players BEFORE pagination (no N+1!)
            var clubs = await query
                .Include(c => c.Players)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var clubDtos = clubs.Select(c => new ClubDto
            {
                Id = c.Id,
                Name = c.Name,
                City = c.City,
                LogoUrl = c.LogoUrl,
                FoundedYear = c.FoundedYear,
                President = c.President,
                Budget = c.Budget,
                PlayerCount = c.Players?.Count ?? 0,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            _logger.LogInformation(
                $"Retrieved {clubs.Count} clubs. Total: {totalCount}, Pages: {totalPages}");

            return new ClubListResponseDto
            {
                Data = clubDtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<ClubDetailDto?> GetClubByIdAsync(int id)
        {
            _logger.LogInformation($"Retrieving club details: ID={id}");

            var club = await _context.Clubs
                .Include(c => c.Players)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null)
            {
                _logger.LogWarning($"Club not found: ID={id}");
                return null;
            }

            var detailDto = new ClubDetailDto
            {
                Id = club.Id,
                Name = club.Name,
                City = club.City,
                LogoUrl = club.LogoUrl,
                FoundedYear = club.FoundedYear,
                President = club.President,
                Budget = club.Budget,
                Players = club.Players?.Select(p => new ClubPlayerSummaryDto
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Position = p.Position,
                    JerseyNumber = p.JerseyNumber
                }).ToList() ?? new List<ClubPlayerSummaryDto>(),
                PlayerCount = club.Players?.Count ?? 0,
                CreatedAt = club.CreatedAt,
                UpdatedAt = club.UpdatedAt
            };

            _logger.LogInformation($"Club details retrieved: {club.Name}");
            return detailDto;
        }

        public async Task<ClubDto> CreateClubAsync(CreateClubDto createClubDto)
        {
            _logger.LogInformation($"Creating club: {createClubDto.Name}");

            // ✅ INPUT SANITIZATION: Trim whitespace
            var trimmedName = createClubDto.Name?.Trim() ?? string.Empty;
            var trimmedCity = createClubDto.City?.Trim() ?? string.Empty;
            var trimmedPresident = createClubDto.President?.Trim();

            // ✅ VALIDATION: Required fields
            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                _logger.LogWarning("Club creation failed: Name is empty after trim");
                throw new InvalidOperationException("Club name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(trimmedCity))
            {
                _logger.LogWarning("Club creation failed: City is empty after trim");
                throw new InvalidOperationException("City cannot be empty");
            }

            // ✅ VALIDATION: Founded year
            ValidateFoundedYear(createClubDto.FoundedYear);

            // ✅ VALIDATION: Budget
            ValidateBudget(createClubDto.Budget);

            // ✅ VALIDATION: Unique name
            if (!await IsClubNameUniqueAsync(trimmedName))
            {
                _logger.LogWarning($"Duplicate club name: {trimmedName}");
                throw new InvalidOperationException(
                    $"A club with name '{trimmedName}' already exists");
            }

            var club = new Club
            {
                Name = trimmedName,
                City = trimmedCity,
                LogoUrl = createClubDto.LogoUrl?.Trim(),
                FoundedYear = createClubDto.FoundedYear,
                President = trimmedPresident,
                Budget = createClubDto.Budget,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Club created successfully: {club.Name} (ID={club.Id})");

            return new ClubDto
            {
                Id = club.Id,
                Name = club.Name,
                City = club.City,
                LogoUrl = club.LogoUrl,
                FoundedYear = club.FoundedYear,
                President = club.President,
                Budget = club.Budget,
                PlayerCount = 0,
                CreatedAt = club.CreatedAt,
                UpdatedAt = club.UpdatedAt
            };
        }

        public async Task<ClubDto?> UpdateClubAsync(int id, UpdateClubDto updateClubDto)
        {
            _logger.LogInformation($"Updating club: ID={id}");

            // ✅ Include Players to avoid N+1 problem!
            var club = await _context.Clubs
                .Include(c => c.Players)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null)
            {
                _logger.LogWarning($"Club not found for update: ID={id}");
                return null;
            }

            // ✅ Sanitize all inputs
            var trimmedName = updateClubDto.Name?.Trim();
            var trimmedCity = updateClubDto.City?.Trim();
            var trimmedPresident = updateClubDto.President?.Trim();

            // ✅ Update Name with validation
            if (!string.IsNullOrWhiteSpace(trimmedName) && trimmedName != club.Name)
            {
                if (!await IsClubNameUniqueAsync(trimmedName, id))
                {
                    _logger.LogWarning($"Duplicate name on update: {trimmedName}");
                    throw new InvalidOperationException(
                        $"A club with name '{trimmedName}' already exists");
                }
                club.Name = trimmedName;
                _logger.LogInformation($"Club name updated: {trimmedName}");
            }

            // ✅ Update City
            if (!string.IsNullOrWhiteSpace(trimmedCity))
            {
                club.City = trimmedCity;
                _logger.LogInformation($"Club city updated: {trimmedCity}");
            }

            // ✅ Update LogoUrl
            if (!string.IsNullOrWhiteSpace(updateClubDto.LogoUrl))
            {
                club.LogoUrl = updateClubDto.LogoUrl.Trim();
            }

            // ✅ Update FoundedYear with validation
            if (updateClubDto.FoundedYear.HasValue)
            {
                ValidateFoundedYear(updateClubDto.FoundedYear.Value);
                club.FoundedYear = updateClubDto.FoundedYear.Value;
                _logger.LogInformation($"Club founded year updated: {updateClubDto.FoundedYear}");
            }

            // ✅ Update President
            if (!string.IsNullOrWhiteSpace(trimmedPresident))
            {
                club.President = trimmedPresident;
            }

            // ✅ Update Budget with validation
            if (updateClubDto.Budget.HasValue)
            {
                ValidateBudget(updateClubDto.Budget);
                club.Budget = updateClubDto.Budget;
                _logger.LogInformation($"Club budget updated: {updateClubDto.Budget}");
            }

            // ✅ Update timestamp
            club.UpdatedAt = DateTime.UtcNow;

            _context.Clubs.Update(club);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Club updated successfully: {club.Name} (ID={club.Id})");

            // ✅ Return correct PlayerCount (Players already loaded via Include)
            return new ClubDto
            {
                Id = club.Id,
                Name = club.Name,
                City = club.City,
                LogoUrl = club.LogoUrl,
                FoundedYear = club.FoundedYear,
                President = club.President,
                Budget = club.Budget,
                PlayerCount = club.Players?.Count ?? 0,
                CreatedAt = club.CreatedAt,
                UpdatedAt = club.UpdatedAt
            };
        }

        public async Task<bool> DeleteClubAsync(int id)
        {
            _logger.LogInformation($"Deleting club: ID={id}");

            var club = await _context.Clubs.FindAsync(id);
            if (club == null)
            {
                _logger.LogWarning($"Club not found for deletion: ID={id}");
                return false;
            }

            _context.Clubs.Remove(club);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Club deleted successfully: {club.Name} (ID={club.Id})");
            return true;
        }

        public async Task<int> GetClubsCountAsync()
        {
            var count = await _context.Clubs.CountAsync();
            _logger.LogInformation($"Total clubs count: {count}");
            return count;
        }
    }
}
