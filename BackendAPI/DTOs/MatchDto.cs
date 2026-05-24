namespace FootballClubAPI.DTOs
{
    public class MatchDto
    {
        public int Id { get; set; }
        public string HomeClubName { get; set; } = string.Empty;
        public string AwayClubName { get; set; } = string.Empty;
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public DateTime MatchDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CompetitionType { get; set; }
        public string StadiumName { get; set; } = string.Empty;
        public string SeasonName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class MatchDetailDto
    {
        public int Id { get; set; }
        public int HomeClubId { get; set; }
        public string HomeClubName { get; set; } = string.Empty;
        public int AwayClubId { get; set; }
        public string AwayClubName { get; set; } = string.Empty;
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public DateTime MatchDate { get; set; }
        public TimeSpan? Time { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CompetitionType { get; set; }
        public int StadiumId { get; set; }
        public string StadiumName { get; set; } = string.Empty;
        public string? StadiumLocation { get; set; }
        public int SeasonId { get; set; }
        public string SeasonName { get; set; } = string.Empty;
        public List<MatchEventDto> Events { get; set; } = new();
        public List<PlayerStatsDto> Stats { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class MatchEventDto
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public int Minute { get; set; }
        public string? Description { get; set; }
    }

    public class PlayerStatsDto
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int MatchId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int MinutesPlayed { get; set; }
        public int GoalsScored { get; set; }
        public int Assists { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public decimal? Rating { get; set; }
    }

    public class CreateMatchDto
    {
        public int HomeClubId { get; set; }
        public int AwayClubId { get; set; }
        public int StadiumId { get; set; }
        public DateTime MatchDate { get; set; }
        public TimeSpan? Time { get; set; }
        public int SeasonId { get; set; }
        public string? CompetitionType { get; set; }
    }

    public class UpdateMatchDto
    {
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public string? Status { get; set; }
    }
}
