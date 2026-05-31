namespace FootballClubAPI.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalClubs { get; set; }
        public int TotalPlayers { get; set; }
        public int TotalMatches { get; set; }
        public int TotalStaff { get; set; }
        public int TotalInjuries { get; set; }
        public int TotalContracts { get; set; }
    }

    public class DashboardTopScorerDto
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string Club { get; set; } = string.Empty;
        public int Goals { get; set; }
        public int Assists { get; set; }
    }

    public class DashboardUpcomingMatchDto
    {
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Stadium { get; set; } = string.Empty;
    }

    public class DashboardInjuredPlayerDto
    {
        public string PlayerName { get; set; } = string.Empty;
        public string Club { get; set; } = string.Empty;
        public string InjuryType { get; set; } = string.Empty;
        public DateTime InjuryDate { get; set; }
        public DateTime? RecoveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DashboardExpiringContractDto
    {
        public string PlayerName { get; set; } = string.Empty;
        public string Club { get; set; } = string.Empty;
        public DateTime ContractEndDate { get; set; }
    }

    public class DashboardRecentTransferDto
    {
        public string PlayerName { get; set; } = string.Empty;
        public string FromClub { get; set; } = string.Empty;
        public string ToClub { get; set; } = string.Empty;
        public decimal TransferFee { get; set; }
        public DateTime TransferDate { get; set; }
    }
}
