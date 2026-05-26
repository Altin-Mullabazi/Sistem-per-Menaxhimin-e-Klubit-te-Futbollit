using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class Player
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jersey number is required")]
        [Range(1, 99)]
        public int JerseyNumber { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [StringLength(50)]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Nationality is required")]
        [StringLength(100)]
        public string Nationality { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? Height { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? Weight { get; set; }

        public PlayerStatus? Status { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MarketValue { get; set; }

        [ForeignKey("Club")]
        public int? ClubId { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }

        [ForeignKey("CreatedByUser")]
        public string? CreatedById { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Club? Club { get; set; }
        public virtual User? User { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }
        public virtual ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public virtual ICollection<Injury> Injuries { get; set; } = new List<Injury>();
        public virtual ICollection<TrainingAttendance> TrainingAttendances { get; set; } = new List<TrainingAttendance>();
    }
}
