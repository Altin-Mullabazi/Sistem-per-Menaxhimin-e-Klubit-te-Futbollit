using System.ComponentModel.DataAnnotations;
using FootballClubAPI.Models;

namespace FootballClubAPI.DTOs
{
    public class TransferDto
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int FromClubId { get; set; }
        public string FromClubName { get; set; } = string.Empty;
        public int ToClubId { get; set; }
        public string ToClubName { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public decimal TransferFee { get; set; }
        public TransferType Type { get; set; }
        public TransferStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateTransferDto
    {
        [Required]
        public int PlayerId { get; set; }

        [Required]
        public int FromClubId { get; set; }

        [Required]
        public int ToClubId { get; set; }

        [Required]
        public DateTime TransferDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Transfer fee must be greater than or equal to 0.")]
        public decimal TransferFee { get; set; }

        [Required]
        public TransferType Type { get; set; }
    }

    public class UpdateTransferDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "Transfer fee must be greater than or equal to 0.")]
        public decimal TransferFee { get; set; }

        [Required]
        public TransferType Type { get; set; }
    }
}
