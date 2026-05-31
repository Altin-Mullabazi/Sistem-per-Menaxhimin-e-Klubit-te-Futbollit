using System.ComponentModel.DataAnnotations;
using FootballClubAPI.Models;

namespace FootballClubAPI.DTOs
{
    /// <summary>
    /// Transfer DTO for list/get responses
    /// </summary>
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

    /// <summary>
    /// DTO for creating transfers
    /// </summary>
    public class CreateTransferDto
    {
        [Required(ErrorMessage = "PlayerId is required")]
        public int PlayerId { get; set; }

        [Required(ErrorMessage = "FromClubId is required")]
        public int FromClubId { get; set; }

        [Required(ErrorMessage = "ToClubId is required")]
        public int ToClubId { get; set; }

        [Required(ErrorMessage = "TransferDate is required")]
        public DateTime TransferDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Transfer fee must be greater than or equal to 0")]
        public decimal TransferFee { get; set; }

        [Required(ErrorMessage = "Transfer type is required")]
        public TransferType Type { get; set; }
    }

    /// <summary>
    /// DTO for updating transfers (only fee and type can be updated)
    /// </summary>
    public class UpdateTransferDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "Transfer fee must be greater than or equal to 0")]
        public decimal TransferFee { get; set; }

        [Required(ErrorMessage = "Transfer type is required")]
        public TransferType Type { get; set; }
    }
}
