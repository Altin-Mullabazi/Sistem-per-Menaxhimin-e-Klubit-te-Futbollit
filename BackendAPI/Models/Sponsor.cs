using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class Sponsor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Logo { get; set; } // URL

        [StringLength(500)]
        public string? Website { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? UserId { get; set; } // FK to User

        public User? User { get; set; }

        public ICollection<SponsorClub> SponsorClubs { get; set; } = new List<SponsorClub>();
    }
}