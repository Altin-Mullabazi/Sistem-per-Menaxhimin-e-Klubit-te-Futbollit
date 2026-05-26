using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.DTOs
{
    public class PlayerStatsListDto
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int MatchId { get; set; }
        public DateTime MatchDate { get; set; }
        public int GoalsScored { get; set; }
        public int Assists { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public int MinutesPlayed { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TopScorerDto
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int? ClubId { get; set; }
        public string? ClubName { get; set; }
        public int GoalsScored { get; set; }
        public int Assists { get; set; }
    }

    public class CreatePlayerStatsDto
    {
        [Required]
        public int? PlayerId { get; set; }

        [Required]
        public int? MatchId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "GoalsScored must be >= 0")]
        public int GoalsScored { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Assists must be >= 0")]
        public int Assists { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "YellowCards must be >= 0")]
        public int YellowCards { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "RedCards must be >= 0")]
        public int RedCards { get; set; }

        [Required]
        [Range(0, 120, ErrorMessage = "MinutesPlayed must be between 0 and 120")]
        public int MinutesPlayed { get; set; }
    }

    public class UpdatePlayerStatsDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "GoalsScored must be >= 0")]
        public int GoalsScored { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Assists must be >= 0")]
        public int Assists { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "YellowCards must be >= 0")]
        public int YellowCards { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "RedCards must be >= 0")]
        public int RedCards { get; set; }

        [Required]
        [Range(0, 120, ErrorMessage = "MinutesPlayed must be between 0 and 120")]
        public int MinutesPlayed { get; set; }
    }
}
