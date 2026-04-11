using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class Season
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [StringLength(100)]
        public string? Competition { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }

        public ICollection<Match> Matches { get; set; } = new HashSet<Match>();
    }
}
