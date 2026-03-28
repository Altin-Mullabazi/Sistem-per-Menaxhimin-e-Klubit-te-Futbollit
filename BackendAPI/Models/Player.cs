using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.Models
{
    public class Player
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required")]
        [Range(16, 45, ErrorMessage = "Age must be between 16 and 45")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [StringLength(50)]
        public string Position { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ClubName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
