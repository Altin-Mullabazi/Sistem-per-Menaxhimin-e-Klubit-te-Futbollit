using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public class StaffService : IStaffService
    {
        private readonly ApplicationDbContext _context;

        public StaffService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<StaffDto> staff, int totalCount)> GetStaffAsync(int page = 1, int pageSize = 10, string? search = null, int? clubId = null)
        {
            var query = _context.Staff.Include(s => s.Club).Include(s => s.User).AsQueryable();

            if (clubId.HasValue)
                query = query.Where(s => s.ClubId == clubId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(s => s.FirstName.ToLower().Contains(lower) || s.LastName.ToLower().Contains(lower));
            }

            var totalCount = await query.CountAsync();

            var staff = await query.OrderBy(s => s.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (staff.Select(MapToDto).ToList(), totalCount);
        }

        public async Task<StaffDto?> GetStaffByIdAsync(int id)
        {
            var staff = await _context.Staff.Include(s => s.Club).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            return staff == null ? null : MapToDto(staff);
        }

        public async Task<StaffDto> CreateStaffAsync(CreateStaffDto createDto)
        {
            var club = await _context.Clubs.FindAsync(createDto.ClubId);
            if (club == null) throw new InvalidOperationException("Club not found.");

            var user = await _context.Users.FindAsync(createDto.UserId);
            if (user == null) throw new InvalidOperationException("User not found.");

            var staff = new Staff
            {
                ClubId = createDto.ClubId,
                UserId = createDto.UserId,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Role = createDto.Role,
                Specialization = createDto.Specialization,
                EmploymentDate = createDto.EmploymentDate,
                Status = createDto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();

            await _context.Entry(staff).Reference(s => s.Club).LoadAsync();
            await _context.Entry(staff).Reference(s => s.User).LoadAsync();

            return MapToDto(staff);
        }

        public async Task<StaffDto?> UpdateStaffAsync(int id, UpdateStaffDto updateDto)
        {
            var staff = await _context.Staff.Include(s => s.Club).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (staff == null) return null;

            if (updateDto.FirstName != null) staff.FirstName = updateDto.FirstName;
            if (updateDto.LastName != null) staff.LastName = updateDto.LastName;
            if (updateDto.Role != null) staff.Role = updateDto.Role;
            if (updateDto.Specialization != null) staff.Specialization = updateDto.Specialization;
            if (updateDto.EmploymentDate.HasValue) staff.EmploymentDate = updateDto.EmploymentDate.Value;
            if (updateDto.Status != null) staff.Status = updateDto.Status;
            if (updateDto.ClubId.HasValue) staff.ClubId = updateDto.ClubId.Value;
            if (!string.IsNullOrEmpty(updateDto.UserId)) staff.UserId = updateDto.UserId;

            staff.UpdatedAt = DateTime.UtcNow;
            _context.Staff.Update(staff);
            await _context.SaveChangesAsync();

            return MapToDto(staff);
        }

        public async Task<bool> DeleteStaffAsync(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null) return false;

            _context.Staff.Remove(staff);
            await _context.SaveChangesAsync();
            return true;
        }

        private static StaffDto MapToDto(Staff s)
        {
            return new StaffDto
            {
                Id = s.Id,
                ClubId = s.ClubId,
                ClubName = s.Club?.Name ?? string.Empty,
                UserId = s.UserId,
                UserEmail = s.User?.Email,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Role = s.Role,
                Specialization = s.Specialization,
                EmploymentDate = s.EmploymentDate,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            };
        }
    }
}
