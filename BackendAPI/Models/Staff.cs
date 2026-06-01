using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class Staff
    {
        public int Id { get; set; }

        [ForeignKey("Club")]
        public int? ClubId { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Role { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Specialization { get; set; }

        public DateTime EmploymentDate { get; set; } = DateTime.UtcNow;

        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Club? Club { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
