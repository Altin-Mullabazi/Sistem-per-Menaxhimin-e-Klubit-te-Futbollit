using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class TrainingSession
    {
        public int Id { get; set; }

        public int ClubId { get; set; }

        [Required]
        public DateTime SessionDate { get; set; }

        [Range(15, 180, 
            ErrorMessage = "Training duration must be between 15 and 180 minutes.")]
        public int Duration { get; set; }

        [Required]
        public TrainingType Type { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // ✅ Track when session details change
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Club Club { get; set; } = null!;
        public ICollection<TrainingAttendance> Attendances { get; set; } = new List<TrainingAttendance>();
    }
}