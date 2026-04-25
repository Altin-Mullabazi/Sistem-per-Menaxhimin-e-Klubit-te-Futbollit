using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class SponsorClub
    {
        public int Id { get; set; } // Primary key for junction table

        [Required]
        public int SponsorId { get; set; }

        [Required]
        public int ClubId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // Navigation properties
        public virtual Sponsor? Sponsor { get; set; }
        public virtual Club? Club { get; set; }
    }
}
