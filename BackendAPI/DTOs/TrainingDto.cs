using System.ComponentModel.DataAnnotations;
using FootballClubAPI.Models;

namespace FootballClubAPI.DTOs
{
    public class TrainingSessionDto
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public DateTime SessionDate { get; set; }
        public int Duration { get; set; }
        public TrainingType Type { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateTrainingSessionDto
    {
        [Required]
        public int ClubId { get; set; }

        [Required]
        public DateTime SessionDate { get; set; }

        [Range(15, 180, ErrorMessage = "Training duration must be between 15 and 180 minutes.")]
        public int Duration { get; set; }

        [Required]
        public TrainingType Type { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class UpdateTrainingSessionDto
    {
        public DateTime? SessionDate { get; set; }

        [Range(15, 180, ErrorMessage = "Training duration must be between 15 and 180 minutes.")]
        public int? Duration { get; set; }

        public TrainingType? Type { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
