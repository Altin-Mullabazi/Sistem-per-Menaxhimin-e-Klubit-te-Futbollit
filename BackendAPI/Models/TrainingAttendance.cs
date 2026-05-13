using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class TrainingAttendance
    {
        public int Id { get; set; }

        public int TrainingSessionId { get; set; }
        public int PlayerId { get; set; }

        public bool Attended { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public TrainingSession TrainingSession { get; set; } = null!;
        public Player Player { get; set; } = null!;
    }
}