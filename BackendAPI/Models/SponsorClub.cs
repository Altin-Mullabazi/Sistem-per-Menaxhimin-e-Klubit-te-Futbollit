using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class SponsorClub
    {
        public int Id { get; set; } // Primary key for junction table

        [Required]
        public int SponsorId { get; set; }

        public Sponsor? Sponsor { get; set; }

        [Required]
        public int ClubId { get; set; }

        public Club? Club { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}