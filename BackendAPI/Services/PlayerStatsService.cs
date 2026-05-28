using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public interface IPlayerStatsService
    {
        Task<(List<PlayerStatsListDto> stats, int totalCount)> GetPlayerStatsAsync(int page = 1, int pageSize = 10);
        Task<List<TopScorerDto>> GetTopScorersAsync(int limit = 10);
        Task<PlayerStatsListDto> CreatePlayerStatsAsync(CreatePlayerStatsDto createDto);
        Task<PlayerStatsListDto?> UpdatePlayerStatsAsync(int id, UpdatePlayerStatsDto updateDto);
        Task<bool> DeletePlayerStatsAsync(int id);
    }

    public class PlayerStatsService : IPlayerStatsService
    {
        private readonly ApplicationDbContext _context;

        public PlayerStatsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<PlayerStatsListDto> stats, int totalCount)> GetPlayerStatsAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.PlayerStats
                .Include(ps => ps.Player)
                .Include(ps => ps.Match)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var stats = await query
                .OrderByDescending(ps => ps.GoalsScored)
                .ThenByDescending(ps => ps.Assists)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (stats.Select(MapToListDto).ToList(), totalCount);
        }

        public async Task<List<TopScorerDto>> GetTopScorersAsync(int limit = 10)
        {
            var stats = await _context.PlayerStats
                .Include(ps => ps.Player)
                    .ThenInclude(p => p.Club)
                .OrderByDescending(ps => ps.GoalsScored)
                .ThenByDescending(ps => ps.Assists)
                .Take(limit)
                .ToListAsync();

            return stats.Select(ps => new TopScorerDto
            {
                PlayerId = ps.PlayerId,
                PlayerName = ps.Player != null ? $"{ps.Player!.FirstName} {ps.Player.LastName}" : string.Empty,
                ClubId = ps.Player?.ClubId,
                ClubName = ps.Player?.Club?.Name,
                GoalsScored = ps.GoalsScored,
                Assists = ps.Assists
            }).ToList();
        }

        public async Task<PlayerStatsListDto> CreatePlayerStatsAsync(CreatePlayerStatsDto createDto)
        {
            ValidateStatsValues(createDto.GoalsScored, createDto.Assists, createDto.YellowCards, createDto.RedCards, createDto.MinutesPlayed);

            var matchId = createDto.MatchId ?? throw new InvalidOperationException("MatchId is required");
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null)
                throw new InvalidOperationException("Match not found");

            var playerId = createDto.PlayerId ?? throw new InvalidOperationException("PlayerId is required");

            var player = await _context.Players.Include(p => p.Club).FirstOrDefaultAsync(p => p.Id == playerId);
            if (player == null)
                throw new InvalidOperationException("Player not found");

            if (player.ClubId != match.HomeClubId && player.ClubId != match.AwayClubId)
                throw new InvalidOperationException("Player is not part of the match");

            if (await _context.PlayerStats.AnyAsync(ps => ps.PlayerId == createDto.PlayerId && ps.MatchId == createDto.MatchId))
                throw new InvalidOperationException("Player stats already exist for this match");

            var stats = new PlayerStats
            {
                PlayerId = createDto.PlayerId.Value,
                MatchId = createDto.MatchId.Value,
                GoalsScored = createDto.GoalsScored,
                Assists = createDto.Assists,
                YellowCards = createDto.YellowCards,
                RedCards = createDto.RedCards,
                MinutesPlayed = createDto.MinutesPlayed,
                CreatedAt = DateTime.UtcNow
            };

            _context.PlayerStats.Add(stats);
            await _context.SaveChangesAsync();

            await _context.Entry(stats).Reference(ps => ps.Player).LoadAsync();
            await _context.Entry(stats).Reference(ps => ps.Match).LoadAsync();

            return MapToListDto(stats);
        }

        public async Task<PlayerStatsListDto?> UpdatePlayerStatsAsync(int id, UpdatePlayerStatsDto updateDto)
        {
            var stats = await _context.PlayerStats
                .Include(ps => ps.Player)
                .Include(ps => ps.Match)
                .FirstOrDefaultAsync(ps => ps.Id == id);

            if (stats == null)
                return null;

            ValidateStatsValues(updateDto.GoalsScored, updateDto.Assists, updateDto.YellowCards, updateDto.RedCards, updateDto.MinutesPlayed);

            stats.GoalsScored = updateDto.GoalsScored;
            stats.Assists = updateDto.Assists;
            stats.YellowCards = updateDto.YellowCards;
            stats.RedCards = updateDto.RedCards;
            stats.MinutesPlayed = updateDto.MinutesPlayed;

            _context.PlayerStats.Update(stats);
            await _context.SaveChangesAsync();

            return MapToListDto(stats);
        }

        public async Task<bool> DeletePlayerStatsAsync(int id)
        {
            var stats = await _context.PlayerStats.FindAsync(id);
            if (stats == null)
                return false;

            _context.PlayerStats.Remove(stats);
            await _context.SaveChangesAsync();

            return true;
        }

        private static void ValidateStatsValues(int goals, int assists, int yellowCards, int redCards, int minutes)
        {
            if (goals < 0 || assists < 0 || yellowCards < 0 || redCards < 0)
                throw new InvalidOperationException("Goals, assists, yellow cards, and red cards must be zero or greater");

            if (minutes < 0 || minutes > 120)
                throw new InvalidOperationException("MinutesPlayed must be between 0 and 120");
        }

        private static PlayerStatsListDto MapToListDto(PlayerStats stats)
        {
            return new PlayerStatsListDto
            {
                Id = stats.Id,
                PlayerId = stats.PlayerId,
                PlayerName = stats.Player != null ? $"{stats.Player.FirstName} {stats.Player.LastName}" : string.Empty,
                MatchId = stats.MatchId,
                MatchDate = stats.Match?.MatchDate ?? DateTime.MinValue,
                GoalsScored = stats.GoalsScored,
                Assists = stats.Assists,
                YellowCards = stats.YellowCards,
                RedCards = stats.RedCards,
                MinutesPlayed = stats.MinutesPlayed,
                CreatedAt = stats.CreatedAt
            };
        }
    }
}
