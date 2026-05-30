using FootballClubAPI.DTOs;

namespace FootballClubAPI.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
        Task<List<DashboardTopScorerDto>> GetTopScorersAsync(int limit = 10);
        Task<List<DashboardUpcomingMatchDto>> GetUpcomingMatchesAsync(int days = 7);
        Task<List<DashboardInjuredPlayerDto>> GetInjuredPlayersAsync();
        Task<List<DashboardExpiringContractDto>> GetExpiringContractsAsync(int days = 90);
        Task<List<DashboardRecentTransferDto>> GetRecentTransfersAsync(int days = 30);
    }

    public class DashboardService : IDashboardService
    {
        public const int MaxLimit = 100;
        public const int MaxDays = 365;

        private readonly IDashboardRepository _repository;

        public DashboardService(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public Task<DashboardSummaryDto> GetSummaryAsync()
        {
            return _repository.GetSummaryAsync();
        }

        public Task<List<DashboardTopScorerDto>> GetTopScorersAsync(int limit = 10)
        {
            ValidateLimit(limit);
            return _repository.GetTopScorersAsync(limit);
        }

        public Task<List<DashboardUpcomingMatchDto>> GetUpcomingMatchesAsync(int days = 7)
        {
            ValidateDays(days);
            return _repository.GetUpcomingMatchesAsync(DateTime.UtcNow.Date, days);
        }

        public Task<List<DashboardInjuredPlayerDto>> GetInjuredPlayersAsync()
        {
            return _repository.GetInjuredPlayersAsync();
        }

        public Task<List<DashboardExpiringContractDto>> GetExpiringContractsAsync(int days = 90)
        {
            ValidateDays(days);
            return _repository.GetExpiringContractsAsync(DateTime.UtcNow.Date, days);
        }

        public Task<List<DashboardRecentTransferDto>> GetRecentTransfersAsync(int days = 30)
        {
            ValidateDays(days);
            return _repository.GetRecentTransfersAsync(DateTime.UtcNow.Date, days);
        }

        private static void ValidateLimit(int limit)
        {
            if (limit < 1 || limit > MaxLimit)
            {
                throw new ArgumentOutOfRangeException(nameof(limit), $"Limit must be between 1 and {MaxLimit}.");
            }
        }

        private static void ValidateDays(int days)
        {
            if (days < 1 || days > MaxDays)
            {
                throw new ArgumentOutOfRangeException(nameof(days), $"Days must be between 1 and {MaxDays}.");
            }
        }
    }
}
