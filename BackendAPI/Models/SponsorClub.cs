using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class SponsorClub
    {
        public int Id { get; set; }

        [Required]
        public int SponsorId { get; set; }

        public virtual Sponsor? Sponsor { get; set; }

        [Required]
        public int ClubId { get; set; }

        public virtual Club? Club { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
