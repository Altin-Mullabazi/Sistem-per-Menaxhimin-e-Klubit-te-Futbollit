using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class Stadium
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Match> Matches { get; set; } = new HashSet<Match>();
    }
}
