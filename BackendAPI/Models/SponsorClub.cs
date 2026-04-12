using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class SponsorClub
    {
        [Key]
        [Column(Order = 1)]
        [ForeignKey("Sponsor")]
        public int SponsorId { get; set; }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("Club")]
        public int ClubId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Sponsor? Sponsor { get; set; }
        public virtual Club? Club { get; set; }
    }
}
