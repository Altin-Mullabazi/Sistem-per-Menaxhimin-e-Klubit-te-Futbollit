using System.Collections.Generic;
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

    public class CreateInjuryDto : IValidatableObject
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (InjuryDate > DateTime.UtcNow.Date)
            {
                yield return new ValidationResult(
                    "InjuryDate cannot be in the future",
                    new[] { nameof(InjuryDate) });
            }
        }
    }

    public class UpdateInjuryDto
    {
        public DateTime? RecoveryDate { get; set; }

        [EnumDataType(typeof(InjuryStatus), ErrorMessage = "Status must be Active, Recovering, or Recovered")]
        public InjuryStatus? Status { get; set; }

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
