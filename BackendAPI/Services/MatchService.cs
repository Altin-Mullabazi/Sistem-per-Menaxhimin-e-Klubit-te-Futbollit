using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public interface IMatchService
    {
        Task<(List<MatchDto> matches, int totalCount)> GetMatchesAsync(int page = 1, int pageSize = 10, int? clubId = null, int? seasonId = null, string? status = null);
        Task<MatchDetailDto?> GetMatchByIdAsync(int id);
        Task<List<MatchDto>> GetUpcomingMatchesAsync(int days = 7);
        Task<MatchDto> CreateMatchAsync(CreateMatchDto createMatchDto);
        Task<MatchDto?> UpdateMatchAsync(int id, UpdateMatchDto updateMatchDto);
        Task<bool> DeleteMatchAsync(int id);
        Task<int> GetMatchesCountAsync();
    }

    public class MatchService : IMatchService
    {
        private readonly ApplicationDbContext _context;

        public MatchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<MatchDto> matches, int totalCount)> GetMatchesAsync(int page = 1, int pageSize = 10, int? clubId = null, int? seasonId = null, string? status = null)
        {
            var query = _context.Matches
                .Include(m => m.HomeClub)
                .Include(m => m.AwayClub)
                .Include(m => m.Stadium)
                .Include(m => m.Season)
                .AsQueryable();

            // Apply filters
            if (clubId.HasValue)
            {
                query = query.Where(m => m.HomeClubId == clubId.Value || m.AwayClubId == clubId.Value);
            }

            if (seasonId.HasValue)
            {
                query = query.Where(m => m.SeasonId == seasonId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(m => m.Status == status);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Sort by date (upcoming first)
            var matches = await query
                .OrderBy(m => m.MatchDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (matches.Select(MapToDto).ToList(), totalCount);
        }

        public async Task<MatchDetailDto?> GetMatchByIdAsync(int id)
        {
            var match = await _context.Matches
                .Include(m => m.HomeClub)
                .Include(m => m.AwayClub)
                .Include(m => m.Stadium)
                .Include(m => m.Season)
                .Include(m => m.MatchEvents)
                    .ThenInclude(me => me.Player)
                .Include(m => m.PlayerStats)
                    .ThenInclude(ps => ps.Player)
                .FirstOrDefaultAsync(m => m.Id == id);

            return match == null ? null : MapToDetailDto(match);
        }

        public async Task<List<MatchDto>> GetUpcomingMatchesAsync(int days = 7)
        {
            var futureDate = DateTime.UtcNow.AddDays(days);

            var matches = await _context.Matches
                .Include(m => m.HomeClub)
                .Include(m => m.AwayClub)
                .Include(m => m.Stadium)
                .Include(m => m.Season)
                .Where(m => m.MatchDate >= DateTime.UtcNow && m.MatchDate <= futureDate)
                .OrderBy(m => m.MatchDate)
                .ToListAsync();

            return matches.Select(MapToDto).ToList();
        }

        public async Task<MatchDto> CreateMatchAsync(CreateMatchDto createMatchDto)
        {
            // Validate home team != away team
            if (createMatchDto.HomeClubId == createMatchDto.AwayClubId)
            {
                throw new InvalidOperationException("Home club and away club cannot be the same");
            }

            // Validate date is not in the past
            if (createMatchDto.MatchDate < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Match date cannot be in the past");
            }

            // Verify clubs, stadium, and season exist
            var homeClub = await _context.Clubs.FindAsync(createMatchDto.HomeClubId);
            if (homeClub == null)
                throw new InvalidOperationException("Home club not found");

            var awayClub = await _context.Clubs.FindAsync(createMatchDto.AwayClubId);
            if (awayClub == null)
                throw new InvalidOperationException("Away club not found");

            var stadium = await _context.Stadiums.FindAsync(createMatchDto.StadiumId);
            if (stadium == null)
                throw new InvalidOperationException("Stadium not found");

            var season = await _context.Seasons.FindAsync(createMatchDto.SeasonId);
            if (season == null)
                throw new InvalidOperationException("Season not found");

            var match = new Match
            {
                HomeClubId = createMatchDto.HomeClubId,
                AwayClubId = createMatchDto.AwayClubId,
                StadiumId = createMatchDto.StadiumId,
                MatchDate = createMatchDto.MatchDate,
                Time = createMatchDto.Time,
                SeasonId = createMatchDto.SeasonId,
                CompetitionType = createMatchDto.CompetitionType,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            // Reload to get related entities
            await _context.Entry(match).Reference(m => m.HomeClub).LoadAsync();
            await _context.Entry(match).Reference(m => m.AwayClub).LoadAsync();
            await _context.Entry(match).Reference(m => m.Stadium).LoadAsync();
            await _context.Entry(match).Reference(m => m.Season).LoadAsync();

            return MapToDto(match);
        }

        public async Task<MatchDto?> UpdateMatchAsync(int id, UpdateMatchDto updateMatchDto)
        {
            var match = await _context.Matches
                .Include(m => m.HomeClub)
                .Include(m => m.AwayClub)
                .Include(m => m.Stadium)
                .Include(m => m.Season)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
                return null;

            // Validate scores
            if (updateMatchDto.HomeScore.HasValue && updateMatchDto.HomeScore.Value < 0)
                throw new InvalidOperationException("Home score cannot be negative");

            if (updateMatchDto.AwayScore.HasValue && updateMatchDto.AwayScore.Value < 0)
                throw new InvalidOperationException("Away score cannot be negative");

            // Update only the provided fields
            if (updateMatchDto.HomeScore.HasValue)
                match.HomeScore = updateMatchDto.HomeScore.Value;

            if (updateMatchDto.AwayScore.HasValue)
                match.AwayScore = updateMatchDto.AwayScore.Value;

            if (!string.IsNullOrEmpty(updateMatchDto.Status))
                match.Status = updateMatchDto.Status;

            match.UpdatedAt = DateTime.UtcNow;

            _context.Matches.Update(match);
            await _context.SaveChangesAsync();

            return MapToDto(match);
        }

        public async Task<bool> DeleteMatchAsync(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null)
                return false;

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetMatchesCountAsync()
        {
            return await _context.Matches.CountAsync();
        }

        private static MatchDto MapToDto(Match match)
        {
            return new MatchDto
            {
                Id = match.Id,
                HomeClubName = match.HomeClub?.Name ?? string.Empty,
                AwayClubName = match.AwayClub?.Name ?? string.Empty,
                HomeScore = match.HomeScore,
                AwayScore = match.AwayScore,
                MatchDate = match.MatchDate,
                Status = match.Status,
                CompetitionType = match.CompetitionType,
                StadiumName = match.Stadium?.Name ?? string.Empty,
                SeasonName = match.Season?.Name ?? string.Empty,
                CreatedAt = match.CreatedAt,
                UpdatedAt = match.UpdatedAt
            };
        }

        private static MatchDetailDto MapToDetailDto(Match match)
        {
            return new MatchDetailDto
            {
                Id = match.Id,
                HomeClubId = match.HomeClubId,
                HomeClubName = match.HomeClub?.Name ?? string.Empty,
                AwayClubId = match.AwayClubId,
                AwayClubName = match.AwayClub?.Name ?? string.Empty,
                HomeScore = match.HomeScore,
                AwayScore = match.AwayScore,
                MatchDate = match.MatchDate,
                Time = match.Time,
                Status = match.Status,
                CompetitionType = match.CompetitionType,
                StadiumId = match.StadiumId,
                StadiumName = match.Stadium?.Name ?? string.Empty,
                StadiumLocation = match.Stadium?.City,
                SeasonId = match.SeasonId,
                SeasonName = match.Season?.Name ?? string.Empty,
                Events = match.MatchEvents?.Select(me => new MatchEventDto
                {
                    Id = me.Id,
                    PlayerId = me.PlayerId,
                    PlayerName = me.Player != null ? $"{me.Player.FirstName} {me.Player.LastName}" : string.Empty,
                    EventType = me.EventType.ToString(),
                    Minute = me.Minute,
                    Description = me.Description
                }).ToList() ?? new(),
                Stats = match.PlayerStats?.Select(ps => new PlayerStatsDto
                {
                    Id = ps.Id,
                    PlayerId = ps.PlayerId,
                    PlayerName = ps.Player != null ? $"{ps.Player.FirstName} {ps.Player.LastName}" : string.Empty,
                    MinutesPlayed = ps.MinutesPlayed,
                    GoalsScored = ps.GoalsScored,
                    Assists = ps.Assists,
                    YellowCards = ps.YellowCards,
                    RedCards = ps.RedCards,
                    Rating = ps.Rating
                }).ToList() ?? new(),
                CreatedAt = match.CreatedAt,
                UpdatedAt = match.UpdatedAt
            };
        }
    }
}
