using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class Contract
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public int ClubId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Range(typeof(decimal), "0.01", "999999999.99", 
            ErrorMessage = "Salary must be greater than 0.")]
        public decimal Salary { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // ✅ ONLY Status (no IsActive!)
        // This enforces single source of truth
        // Unique index ensures only 1 Active per player
        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        [StringLength(1000)]
        public string? Terms { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // ✅ Set explicitly in service (not default)
        public DateTime UpdatedAt { get; set; }

        public Player Player { get; set; } = null!;
        public Club Club { get; set; } = null!;
    }
}