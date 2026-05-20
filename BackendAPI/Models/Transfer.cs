using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballClubAPI.Models
{
    public class Transfer
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public int FromClubId { get; set; }
        public int ToClubId { get; set; }

        [Required]
        public DateTime TransferDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Range(typeof(decimal), "0.00", "999999999.99", 
            ErrorMessage = "Transfer fee must be greater than or equal to 0.")]
        public decimal TransferFee { get; set; }

        [Required]
        public TransferType Type { get; set; } = TransferType.Permanent;

        [Required]
        public TransferStatus Status { get; set; } = TransferStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // ✅ Track when transfer status changes
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Player Player { get; set; } = null!;
        public Club FromClub { get; set; } = null!;
        public Club ToClub { get; set; } = null!;
    }
}