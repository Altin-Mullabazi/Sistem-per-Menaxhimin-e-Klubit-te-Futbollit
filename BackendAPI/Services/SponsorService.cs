using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public interface ISponsorService
    {
        Task<PaginatedResult<SponsorDto>> GetSponsorsAsync(int page = 1, int pageSize = 10);
        Task<SponsorDetailDto?> GetSponsorByIdAsync(int id);
        Task<SponsorDto> CreateSponsorAsync(CreateSponsorDto createSponsorDto, string? userId = null);
        Task<SponsorDto?> UpdateSponsorAsync(int id, UpdateSponsorDto updateSponsorDto);
        Task<bool> DeleteSponsorAsync(int id);
    }

    public class SponsorService : ISponsorService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SponsorService> _logger;

        public SponsorService(ApplicationDbContext context, ILogger<SponsorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedResult<SponsorDto>> GetSponsorsAsync(int page = 1, int pageSize = 10)
        {
            _logger.LogInformation("Retrieving sponsors with pagination: page {Page}, pageSize {PageSize}", page, pageSize);

            // Validate and sanitize pagination parameters
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Sponsors
                .Include(s => s.SponsorClubs)
                .ThenInclude(sc => sc.Club)
                .OrderByDescending(s => s.CreatedAt);

            var totalCount = await query.CountAsync();
            var sponsors = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var sponsorDtos = sponsors.Select(MapToDto).ToList();

            _logger.LogInformation("Retrieved {Count} sponsors out of {Total}", sponsorDtos.Count, totalCount);

            return new PaginatedResult<SponsorDto>
            {
                Data = sponsorDtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<SponsorDetailDto?> GetSponsorByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving sponsor with ID {Id}", id);

            var sponsor = await _context.Sponsors
                .Include(s => s.SponsorClubs)
                .ThenInclude(sc => sc.Club)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with ID {Id} not found", id);
                return null;
            }

            _logger.LogInformation("Retrieved sponsor {Name} with {ClubCount} clubs", sponsor.Name, sponsor.SponsorClubs.Count);

            return MapToDetailDto(sponsor);
        }

        public async Task<SponsorDto> CreateSponsorAsync(CreateSponsorDto createSponsorDto, string? userId = null)
        {
            _logger.LogInformation("Creating new sponsor: {Name}", createSponsorDto.Name);

            if (string.IsNullOrWhiteSpace(userId) && _context.Database.IsRelational())
            {
                userId = await _context.LegacyUsers.Select(user => user.Id).FirstOrDefaultAsync();
            }

            if (string.IsNullOrWhiteSpace(userId) && _context.Database.IsRelational())
            {
                throw new InvalidOperationException("Unable to resolve an owner user for the sponsor");
            }

            var sponsor = new Sponsor
            {
                Name = createSponsorDto.Name.Trim(),
                Logo = string.IsNullOrWhiteSpace(createSponsorDto.Logo) ? null : createSponsorDto.Logo.Trim(),
                Website = string.IsNullOrWhiteSpace(createSponsorDto.Website) ? null : createSponsorDto.Website.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId ?? string.Empty
            };

            _context.Sponsors.Add(sponsor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created sponsor with ID {Id}", sponsor.Id);

            return MapToDto(sponsor);
        }

        public async Task<SponsorDto?> UpdateSponsorAsync(int id, UpdateSponsorDto updateSponsorDto)
        {
            _logger.LogInformation("Updating sponsor with ID {Id}", id);

            var sponsor = await _context.Sponsors.FindAsync(id);
            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with ID {Id} not found for update", id);
                return null;
            }

            sponsor.Name = updateSponsorDto.Name.Trim();
            sponsor.Logo = string.IsNullOrWhiteSpace(updateSponsorDto.Logo) ? null : updateSponsorDto.Logo.Trim();
            sponsor.Website = string.IsNullOrWhiteSpace(updateSponsorDto.Website) ? null : updateSponsorDto.Website.Trim();
            sponsor.UpdatedAt = DateTime.UtcNow;

            _context.Sponsors.Update(sponsor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated sponsor {Name}", sponsor.Name);

            return MapToDto(sponsor);
        }

        public async Task<bool> DeleteSponsorAsync(int id)
        {
            _logger.LogInformation("Deleting sponsor with ID {Id}", id);

            var sponsor = await _context.Sponsors.FindAsync(id);
            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with ID {Id} not found for deletion", id);
                return false;
            }

            _context.Sponsors.Remove(sponsor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted sponsor {Name}", sponsor.Name);

            return true;
        }

        private static SponsorDto MapToDto(Sponsor sponsor)
        {
            return new SponsorDto
            {
                Id = sponsor.Id,
                Name = sponsor.Name,
                Logo = sponsor.Logo,
                Website = sponsor.Website,
                CreatedAt = sponsor.CreatedAt,
                UpdatedAt = sponsor.UpdatedAt
            };
        }

        private static SponsorDetailDto MapToDetailDto(Sponsor sponsor)
        {
            return new SponsorDetailDto
            {
                Id = sponsor.Id,
                Name = sponsor.Name,
                Logo = sponsor.Logo,
                Website = sponsor.Website,
                CreatedAt = sponsor.CreatedAt,
                UpdatedAt = sponsor.UpdatedAt,
                Clubs = sponsor.SponsorClubs
                    .Where(sc => sc.Club != null)
                    .Select(sc => new ClubDto
                    {
                        Id = sc.Club!.Id,
                        Name = sc.Club.Name
                    })
                    .ToList()
            };
        }
    }
}