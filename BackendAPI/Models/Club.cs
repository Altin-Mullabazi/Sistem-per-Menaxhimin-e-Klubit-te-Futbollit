using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class Club
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Match> HomeMatches { get; set; } = new HashSet<Match>();
        public ICollection<Match> AwayMatches { get; set; } = new HashSet<Match>();
    }
}
