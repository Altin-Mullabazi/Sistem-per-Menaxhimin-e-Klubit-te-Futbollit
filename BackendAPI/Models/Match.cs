using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class Match
    {
        public int Id { get; set; }

        public int HomeClubId { get; set; }
        public Club? HomeClub { get; set; }

        public int AwayClubId { get; set; }
        public Club? AwayClub { get; set; }

        public DateTime MatchDate { get; set; }
        public TimeSpan? Time { get; set; }

        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CompetitionType { get; set; }

        public int StadiumId { get; set; }
        public Stadium? Stadium { get; set; }

        public int SeasonId { get; set; }
        public Season? Season { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MatchEvent> MatchEvents { get; set; } = new HashSet<MatchEvent>();
        public ICollection<PlayerStats> PlayerStats { get; set; } = new HashSet<PlayerStats>();
    }
}
