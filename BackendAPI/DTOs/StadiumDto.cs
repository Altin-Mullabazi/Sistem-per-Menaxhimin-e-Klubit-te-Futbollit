using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.DTOs
{
    public class StadiumDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int YearBuilt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class StadiumMatchDto
    {
        public int Id { get; set; }
        public int HomeClubId { get; set; }
        public int AwayClubId { get; set; }
        public DateTime MatchDate { get; set; }
        public TimeSpan? Time { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CompetitionType { get; set; }
        public int StadiumId { get; set; }
        public int SeasonId { get; set; }
    }

    public class StadiumDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int YearBuilt { get; set; }
        public List<ClubDto> Clubs { get; set; } = new();
        public List<StadiumMatchDto> Matches { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateStadiumDto
    {
        [Required(ErrorMessage = "Stadium name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Year built is required")]
        [Range(1800, 2100, ErrorMessage = "Year built must be between 1800 and 2100")]
        public int YearBuilt { get; set; }
    }

    public class UpdateStadiumDto
    {
        [Required(ErrorMessage = "Stadium name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Year built is required")]
        [Range(1800, 2100, ErrorMessage = "Year built must be between 1800 and 2100")]
        public int YearBuilt { get; set; }
    }
}
