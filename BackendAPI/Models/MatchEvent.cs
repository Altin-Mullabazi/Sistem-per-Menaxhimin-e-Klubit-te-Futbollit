using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public enum EventType
    {
        Goal,
        YellowCard,
        RedCard,
        Substitution
    }

    public class MatchEvent
    {
        public int Id { get; set; }

        public int MatchId { get; set; }
        public Match? Match { get; set; }

        public int PlayerId { get; set; }
        public Player? Player { get; set; }

        [Required]
        public EventType EventType { get; set; }

        [Range(0, 120)]
        public int Minute { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

