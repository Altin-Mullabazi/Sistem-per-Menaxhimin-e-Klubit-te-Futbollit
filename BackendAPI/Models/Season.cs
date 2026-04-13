using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class Season
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Season name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [StringLength(100)]
        public string? Competition { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual User? User { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }

        public ICollection<Match> Matches { get; set; } = new HashSet<Match>();
    }
}
