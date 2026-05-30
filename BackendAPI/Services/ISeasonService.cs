using FootballClubAPI.DTOs;

namespace FootballClubAPI.Services
{
    public interface ISeasonService
    {
        Task<(IEnumerable<SeasonDto> Seasons, int TotalCount)> GetSeasonsAsync(int page = 1, int pageSize = 10);
        Task<SeasonDto?> GetSeasonByIdAsync(int id);
        Task<SeasonDto> CreateSeasonAsync(CreateSeasonDto createSeasonDto, string? userId = null);
        Task<SeasonDto?> UpdateSeasonAsync(int id, UpdateSeasonDto updateSeasonDto);
        Task<bool> DeleteSeasonAsync(int id);
    }
}
