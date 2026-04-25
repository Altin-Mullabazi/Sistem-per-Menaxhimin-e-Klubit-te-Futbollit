using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class Stadium
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Stadium name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1000, 150000)]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Year built is required")]
        [Range(1800, 2100)]
        public int YearBuilt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Club? Club { get; set; }
        public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
