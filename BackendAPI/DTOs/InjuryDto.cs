using System.ComponentModel.DataAnnotations;
using FootballClubAPI.Models;

namespace FootballClubAPI.DTOs
{
    public class InjuryDto
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public string InjuryType { get; set; } = string.Empty;
        public DateTime InjuryDate { get; set; }
        public DateTime? RecoveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateInjuryDto
    {
        [Required(ErrorMessage = "PlayerId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "PlayerId must be valid")]
        public int PlayerId { get; set; }

        [Required(ErrorMessage = "InjuryType is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "InjuryType must be between 2 and 100 characters")]
        public string InjuryType { get; set; } = string.Empty;

        [Required(ErrorMessage = "InjuryDate is required")]
        public DateTime InjuryDate { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }

    public class UpdateInjuryDto
    {
        public DateTime? RecoveryDate { get; set; }

        [StringLength(100, ErrorMessage = "Status must be valid")]
        public string? Status { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }

    public class PaginatedInjuryResponse
    {
        public List<InjuryDto> Data { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
