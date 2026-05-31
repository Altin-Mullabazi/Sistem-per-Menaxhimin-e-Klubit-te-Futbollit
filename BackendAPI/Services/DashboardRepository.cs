using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public interface IDashboardRepository
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
        Task<List<DashboardTopScorerDto>> GetTopScorersAsync(int limit);
        Task<List<DashboardUpcomingMatchDto>> GetUpcomingMatchesAsync(DateTime utcToday, int days);
        Task<List<DashboardInjuredPlayerDto>> GetInjuredPlayersAsync();
        Task<List<DashboardExpiringContractDto>> GetExpiringContractsAsync(DateTime utcToday, int days);
        Task<List<DashboardRecentTransferDto>> GetRecentTransfersAsync(DateTime utcToday, int days);
    }

    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            return new DashboardSummaryDto
            {
                TotalClubs = await _context.Clubs.CountAsync(),
                TotalPlayers = await _context.Players.CountAsync(),
                TotalMatches = await _context.Matches.CountAsync(),
                TotalStaff = await _context.LegacyUsers.CountAsync(user => user.Role == "Staff"),
                TotalInjuries = await _context.Injuries.CountAsync(),
                TotalContracts = await _context.Contracts.CountAsync()
            };
        }

        public async Task<List<DashboardTopScorerDto>> GetTopScorersAsync(int limit)
        {
            var query =
                from stats in _context.PlayerStats.AsNoTracking()
                join player in _context.Players.AsNoTracking()
                    on stats.PlayerId equals player.Id
                join club in _context.Clubs.AsNoTracking()
                    on player.ClubId equals club.Id into playerClub
                from club in playerClub.DefaultIfEmpty()
                group stats by new
                {
                    PlayerId = player.Id,
                    player.FirstName,
                    player.LastName,
                    ClubName = club != null ? club.Name : string.Empty
                }
                into groupedStats
                select new DashboardTopScorerDto
                {
                    PlayerId = groupedStats.Key.PlayerId,
                    PlayerName = (groupedStats.Key.FirstName + " " + groupedStats.Key.LastName).Trim(),
                    Club = groupedStats.Key.ClubName,
                    Goals = groupedStats.Sum(stats => stats.GoalsScored),
                    Assists = groupedStats.Sum(stats => stats.Assists)
                };

            return await query
                .OrderByDescending(scorer => scorer.Goals)
                .ThenByDescending(scorer => scorer.Assists)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<DashboardUpcomingMatchDto>> GetUpcomingMatchesAsync(DateTime utcToday, int days)
        {
            var endExclusive = utcToday.Date.AddDays(days + 1);

            return await _context.Matches
                .AsNoTracking()
                .Where(match =>
                    match.MatchDate >= utcToday.Date &&
                    match.MatchDate < endExclusive &&
                    match.Status != "Finished" &&
                    match.Status != "finished" &&
                    match.Status != "Completed" &&
                    match.Status != "completed")
                .OrderBy(match => match.MatchDate)
                .Select(match => new DashboardUpcomingMatchDto
                {
                    HomeTeam = match.HomeClub != null ? match.HomeClub.Name : string.Empty,
                    AwayTeam = match.AwayClub != null ? match.AwayClub.Name : string.Empty,
                    Date = match.MatchDate,
                    Stadium = match.Stadium != null ? match.Stadium.Name : string.Empty
                })
                .ToListAsync();
        }

        public async Task<List<DashboardInjuredPlayerDto>> GetInjuredPlayersAsync()
        {
            return await _context.Injuries
                .AsNoTracking()
                .Where(injury => injury.Status != InjuryStatus.Recovered)
                .OrderByDescending(injury => injury.InjuryDate)
                .Select(injury => new DashboardInjuredPlayerDto
                {
                    PlayerName = (injury.Player.FirstName + " " + injury.Player.LastName).Trim(),
                    Club = injury.Player.Club != null ? injury.Player.Club.Name : string.Empty,
                    InjuryType = injury.InjuryType,
                    InjuryDate = injury.InjuryDate,
                    RecoveryDate = injury.RecoveryDate,
                    Status = injury.Status.ToString()
                })
                .ToListAsync();
        }

        public async Task<List<DashboardExpiringContractDto>> GetExpiringContractsAsync(DateTime utcToday, int days)
        {
            var endExclusive = utcToday.Date.AddDays(days + 1);

            return await _context.Contracts
                .AsNoTracking()
                .Where(contract =>
                    contract.EndDate >= utcToday.Date &&
                    contract.EndDate < endExclusive &&
                    contract.Status == ContractStatus.Active)
                .OrderBy(contract => contract.EndDate)
                .Select(contract => new DashboardExpiringContractDto
                {
                    PlayerName = (contract.Player.FirstName + " " + contract.Player.LastName).Trim(),
                    Club = contract.Club.Name,
                    ContractEndDate = contract.EndDate
                })
                .ToListAsync();
        }

        public async Task<List<DashboardRecentTransferDto>> GetRecentTransfersAsync(DateTime utcToday, int days)
        {
            var startDate = utcToday.Date.AddDays(-days);

            return await _context.Transfers
                .AsNoTracking()
                .Where(transfer => transfer.TransferDate >= startDate)
                .OrderByDescending(transfer => transfer.TransferDate)
                .Select(transfer => new DashboardRecentTransferDto
                {
                    PlayerName = (transfer.Player.FirstName + " " + transfer.Player.LastName).Trim(),
                    FromClub = transfer.FromClub.Name,
                    ToClub = transfer.ToClub.Name,
                    TransferFee = transfer.TransferFee,
                    TransferDate = transfer.TransferDate
                })
                .ToListAsync();
        }
    }
}
