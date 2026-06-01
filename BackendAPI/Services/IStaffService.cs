using FootballClubAPI.DTOs;

namespace FootballClubAPI.Services
{
    public interface IStaffService
    {
        Task<(List<StaffDto> staff, int totalCount)> GetStaffAsync(int page = 1, int pageSize = 10, string? search = null, int? clubId = null);
        Task<StaffDto?> GetStaffByIdAsync(int id);
        Task<StaffDto> CreateStaffAsync(CreateStaffDto createDto);
        Task<StaffDto?> UpdateStaffAsync(int id, UpdateStaffDto updateDto);
        Task<bool> DeleteStaffAsync(int id);
    }
}
