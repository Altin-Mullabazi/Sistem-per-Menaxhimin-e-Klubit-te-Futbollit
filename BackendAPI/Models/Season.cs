using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class Season
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Season name is required")]
        [StringLength(100, ErrorMessage = "Season name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Match> Matches { get; set; } = new HashSet<Match>();
    }
}
