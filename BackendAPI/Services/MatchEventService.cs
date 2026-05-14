using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public interface IMatchEventService
    {
        Task<(List<MatchEventListDto> events, int totalCount)> GetMatchEventsAsync(int matchId, int page = 1, int pageSize = 10);
        Task<MatchEventDetailDto?> GetMatchEventByIdAsync(int id);
        Task<(List<MatchEventListDto> events, int totalCount)> GetPlayerEventsAsync(int playerId, int matchId, int page = 1, int pageSize = 10);
        Task<MatchEventListDto> CreateMatchEventAsync(CreateMatchEventDto createEventDto);
        Task<MatchEventListDto?> UpdateMatchEventAsync(int id, UpdateMatchEventDto updateEventDto);
        Task<bool> DeleteMatchEventAsync(int id);
    }

    public class MatchEventService : IMatchEventService
    {
        private readonly ApplicationDbContext _context;

        public MatchEventService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<MatchEventListDto> events, int totalCount)> GetMatchEventsAsync(int matchId, int page = 1, int pageSize = 10)
        {
            var query = _context.MatchEvents
                .Include(me => me.Player)
                .Include(me => me.Match)
                .Where(me => me.MatchId == matchId)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var events = await query
                .OrderBy(me => me.Minute)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (events.Select(MapToListDto).ToList(), totalCount);
        }

        public async Task<MatchEventDetailDto?> GetMatchEventByIdAsync(int id)
        {
            var matchEvent = await _context.MatchEvents
                .Include(me => me.Player)
                .Include(me => me.Match)
                .FirstOrDefaultAsync(me => me.Id == id);

            return matchEvent == null ? null : MapToDetailDto(matchEvent);
        }

        public async Task<(List<MatchEventListDto> events, int totalCount)> GetPlayerEventsAsync(int playerId, int matchId, int page = 1, int pageSize = 10)
        {
            var query = _context.MatchEvents
                .Include(me => me.Player)
                .Include(me => me.Match)
                .Where(me => me.PlayerId == playerId && me.MatchId == matchId)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var events = await query
                .OrderBy(me => me.Minute)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (events.Select(MapToListDto).ToList(), totalCount);
        }

        public async Task<MatchEventListDto> CreateMatchEventAsync(CreateMatchEventDto createEventDto)
        {
            // Validate EventType enum
            if (!Enum.TryParse<EventType>(createEventDto.EventType, ignoreCase: true, out var eventType))
            {
                throw new InvalidOperationException($"Invalid EventType: {createEventDto.EventType}. Valid values are: Goal, YellowCard, RedCard, Substitution");
            }

            // Validate minute range
            if (createEventDto.Minute < 0 || createEventDto.Minute > 120)
            {
                throw new InvalidOperationException("Minute must be between 0 and 120");
            }

            // Verify match exists
            var match = await _context.Matches.FindAsync(createEventDto.MatchId);
            if (match == null)
            {
                throw new InvalidOperationException("Match not found");
            }

            // Verify player exists
            var player = await _context.Players.FindAsync(createEventDto.PlayerId);
            if (player == null)
            {
                throw new InvalidOperationException("Player not found");
            }

            // Verify player is in the match (check if player has any stats or is related to the match)
            // This is a simplified check - you might want to extend this based on your business logic
            var playerInMatch = await _context.PlayerStats
                .Where(ps => ps.PlayerId == createEventDto.PlayerId && ps.MatchId == createEventDto.MatchId)
                .AnyAsync() 
                || await _context.MatchEvents
                .Where(me => me.PlayerId == createEventDto.PlayerId && me.MatchId == createEventDto.MatchId)
                .AnyAsync();

            // For new matches, allow adding first event from a player (they might not have stats yet)
            // Just verify the player exists, which we already did above

            var matchEvent = new MatchEvent
            {
                MatchId = createEventDto.MatchId,
                PlayerId = createEventDto.PlayerId,
                EventType = eventType,
                Minute = createEventDto.Minute,
                Description = createEventDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.MatchEvents.Add(matchEvent);
            await _context.SaveChangesAsync();

            // Reload to get related entities
            await _context.Entry(matchEvent).Reference(me => me.Player).LoadAsync();
            await _context.Entry(matchEvent).Reference(me => me.Match).LoadAsync();

            return MapToListDto(matchEvent);
        }

        public async Task<MatchEventListDto?> UpdateMatchEventAsync(int id, UpdateMatchEventDto updateEventDto)
        {
            var matchEvent = await _context.MatchEvents
                .Include(me => me.Player)
                .Include(me => me.Match)
                .FirstOrDefaultAsync(me => me.Id == id);

            if (matchEvent == null)
                return null;

            // Validate and update EventType if provided
            if (!string.IsNullOrEmpty(updateEventDto.EventType))
            {
                if (!Enum.TryParse<EventType>(updateEventDto.EventType, ignoreCase: true, out var eventType))
                {
                    throw new InvalidOperationException($"Invalid EventType: {updateEventDto.EventType}. Valid values are: Goal, YellowCard, RedCard, Substitution");
                }
                matchEvent.EventType = eventType;
            }

            // Validate and update Minute if provided
            if (updateEventDto.Minute.HasValue)
            {
                if (updateEventDto.Minute.Value < 0 || updateEventDto.Minute.Value > 120)
                {
                    throw new InvalidOperationException("Minute must be between 0 and 120");
                }
                matchEvent.Minute = updateEventDto.Minute.Value;
            }

            // Update description if provided
            if (updateEventDto.Description != null)
            {
                matchEvent.Description = updateEventDto.Description;
            }

            _context.MatchEvents.Update(matchEvent);
            await _context.SaveChangesAsync();

            return MapToListDto(matchEvent);
        }

        public async Task<bool> DeleteMatchEventAsync(int id)
        {
            var matchEvent = await _context.MatchEvents.FindAsync(id);
            if (matchEvent == null)
                return false;

            _context.MatchEvents.Remove(matchEvent);
            await _context.SaveChangesAsync();

            return true;
        }

        private static MatchEventListDto MapToListDto(MatchEvent matchEvent)
        {
            return new MatchEventListDto
            {
                Id = matchEvent.Id,
                MatchId = matchEvent.MatchId,
                PlayerId = matchEvent.PlayerId,
                PlayerName = matchEvent.Player != null ? $"{matchEvent.Player.FirstName} {matchEvent.Player.LastName}" : string.Empty,
                EventType = matchEvent.EventType.ToString(),
                Minute = matchEvent.Minute,
                Description = matchEvent.Description,
                CreatedAt = matchEvent.CreatedAt
            };
        }

        private static MatchEventDetailDto MapToDetailDto(MatchEvent matchEvent)
        {
            return new MatchEventDetailDto
            {
                Id = matchEvent.Id,
                MatchId = matchEvent.MatchId,
                PlayerId = matchEvent.PlayerId,
                PlayerName = matchEvent.Player != null ? $"{matchEvent.Player.FirstName} {matchEvent.Player.LastName}" : string.Empty,
                PlayerPosition = matchEvent.Player?.Position ?? string.Empty,
                EventType = matchEvent.EventType.ToString(),
                Minute = matchEvent.Minute,
                Description = matchEvent.Description,
                CreatedAt = matchEvent.CreatedAt
            };
        }
    }
}
