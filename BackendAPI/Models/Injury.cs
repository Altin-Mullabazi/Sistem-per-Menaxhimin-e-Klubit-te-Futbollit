using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class Injury
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }

        [Required]
        [StringLength(100)]
        public string InjuryType { get; set; } = string.Empty;

        [Required]
        public DateTime InjuryDate { get; set; }

        public DateTime? RecoveryDate { get; set; }

        [Required]
        public InjuryStatus Status { get; set; } = InjuryStatus.Active;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // ✅ Track when status/recovery changes
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Player Player { get; set; } = null!;
    }
}