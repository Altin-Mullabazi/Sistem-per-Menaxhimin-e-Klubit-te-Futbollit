using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.DTOs
{
    public class SeasonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateSeasonDto
    {
        [Required(ErrorMessage = "Season name is required")]
        [StringLength(100, ErrorMessage = "Season name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class UpdateSeasonDto
    {
        [Required(ErrorMessage = "Season name is required")]
        [StringLength(100, ErrorMessage = "Season name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }
}
