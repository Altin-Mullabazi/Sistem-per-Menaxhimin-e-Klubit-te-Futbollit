using FootballClubAPI.DTOs;

namespace FootballClubAPI.Services
{
    public interface ITrainingService
    {
        Task<(List<TrainingSessionDto> sessions, int totalCount)> GetTrainingSessionsAsync(
            int page = 1,
            int pageSize = 10,
            int? clubId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<TrainingSessionDto?> GetTrainingSessionByIdAsync(int id);
        Task<TrainingSessionDto> CreateTrainingSessionAsync(CreateTrainingSessionDto createDto);
        Task<TrainingSessionDto?> UpdateTrainingSessionAsync(int id, UpdateTrainingSessionDto updateDto);
        Task<bool> DeleteTrainingSessionAsync(int id);
    }
}
