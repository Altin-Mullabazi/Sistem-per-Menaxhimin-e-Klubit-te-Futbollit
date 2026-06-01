using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly ApplicationDbContext _context;

        public TrainingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<TrainingSessionDto> sessions, int totalCount)> GetTrainingSessionsAsync(
            int page = 1,
            int pageSize = 10,
            int? clubId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.TrainingSessions
                .Include(ts => ts.Club)
                .AsQueryable();

            if (clubId.HasValue)
            {
                query = query.Where(ts => ts.ClubId == clubId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(ts => ts.SessionDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(ts => ts.SessionDate <= endOfDay);
            }

            var totalCount = await query.CountAsync();

            var sessions = await query
                .OrderBy(ts => ts.SessionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (sessions.Select(MapToDto).ToList(), totalCount);
        }

        public async Task<TrainingSessionDto?> GetTrainingSessionByIdAsync(int id)
        {
            var session = await _context.TrainingSessions
                .Include(ts => ts.Club)
                .FirstOrDefaultAsync(ts => ts.Id == id);

            return session == null ? null : MapToDto(session);
        }

        public async Task<TrainingSessionDto> CreateTrainingSessionAsync(CreateTrainingSessionDto createDto)
        {
            if (createDto.SessionDate < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Training session date cannot be in the past.");
            }

            var club = await _context.Clubs.FindAsync(createDto.ClubId);
            if (club == null)
            {
                throw new InvalidOperationException("Club not found.");
            }

            var session = new TrainingSession
            {
                ClubId = createDto.ClubId,
                SessionDate = createDto.SessionDate,
                Duration = createDto.Duration,
                Type = createDto.Type,
                Notes = createDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TrainingSessions.Add(session);
            await _context.SaveChangesAsync();

            await _context.Entry(session).Reference(ts => ts.Club).LoadAsync();
            return MapToDto(session);
        }

        public async Task<TrainingSessionDto?> UpdateTrainingSessionAsync(int id, UpdateTrainingSessionDto updateDto)
        {
            var session = await _context.TrainingSessions
                .Include(ts => ts.Club)
                .FirstOrDefaultAsync(ts => ts.Id == id);

            if (session == null)
                return null;

            if (updateDto.SessionDate.HasValue && updateDto.SessionDate.Value.Date < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Training session date cannot be in the past.");
            }

            if (updateDto.SessionDate.HasValue)
                session.SessionDate = updateDto.SessionDate.Value;

            if (updateDto.Duration.HasValue)
                session.Duration = updateDto.Duration.Value;

            if (updateDto.Type.HasValue)
                session.Type = updateDto.Type.Value;

            if (updateDto.Notes != null)
                session.Notes = updateDto.Notes;

            session.UpdatedAt = DateTime.UtcNow;

            _context.TrainingSessions.Update(session);
            await _context.SaveChangesAsync();

            return MapToDto(session);
        }

        public async Task<bool> DeleteTrainingSessionAsync(int id)
        {
            var session = await _context.TrainingSessions.FindAsync(id);
            if (session == null)
                return false;

            _context.TrainingSessions.Remove(session);
            await _context.SaveChangesAsync();
            return true;
        }

        private static TrainingSessionDto MapToDto(TrainingSession session)
        {
            return new TrainingSessionDto
            {
                Id = session.Id,
                ClubId = session.ClubId,
                ClubName = session.Club?.Name ?? string.Empty,
                SessionDate = session.SessionDate,
                Duration = session.Duration,
                Type = session.Type,
                Notes = session.Notes,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt
            };
        }
    }
}
