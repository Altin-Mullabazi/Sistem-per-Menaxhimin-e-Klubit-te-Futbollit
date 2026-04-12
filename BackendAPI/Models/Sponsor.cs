using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class Sponsor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Sponsor name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Logo { get; set; }

        [StringLength(256)]
        public string? Website { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ICollection<SponsorClub> SponsorClubs { get; set; } = new List<SponsorClub>();
    }
}
