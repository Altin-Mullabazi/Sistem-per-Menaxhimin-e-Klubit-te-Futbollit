using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class Club
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Club name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [Required(ErrorMessage = "Founded year is required")]
        [Range(1800, 2100)]
        public int FoundedYear { get; set; }

        [StringLength(100)]
        public string? President { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Budget { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string? UserId { get; set; }

        [ForeignKey("CreatedByUser")]
        public string? CreatedById { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
        public virtual ICollection<Stadium> Stadiums { get; set; } = new List<Stadium>();
        public virtual ICollection<SponsorClub> SponsorClubs { get; set; } = new List<SponsorClub>();
        public virtual ICollection<ClubTrophy> ClubTrophies { get; set; } = new List<ClubTrophy>();
        public virtual ICollection<Match> HomeMatches { get; set; } = new List<Match>();
        public virtual ICollection<Match> AwayMatches { get; set; } = new List<Match>();
        public virtual ICollection<Transfer> OutgoingTransfers { get; set; } = new List<Transfer>();
        public virtual ICollection<Transfer> IncomingTransfers { get; set; } = new List<Transfer>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
        public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
    }
}
