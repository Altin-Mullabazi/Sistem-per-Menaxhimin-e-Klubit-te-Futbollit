using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class Club
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Transfer> OutgoingTransfers { get; set; } = new List<Transfer>();
        public virtual ICollection<Transfer> IncomingTransfers { get; set; } = new List<Transfer>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
    }
}