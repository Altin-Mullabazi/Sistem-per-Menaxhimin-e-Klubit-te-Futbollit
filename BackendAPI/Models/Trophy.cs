using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class Trophy
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Trophy name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Image { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ICollection<ClubTrophy> ClubTrophies { get; set; } = new List<ClubTrophy>();
    }
}
