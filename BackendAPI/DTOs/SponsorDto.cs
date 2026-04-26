using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.DTOs
{
    public class SponsorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public string? Website { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SponsorDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public string? Website { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ClubDto> Clubs { get; set; } = new List<ClubDto>();
    }

    public class ClubDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CreateSponsorDto
    {
        [Required(ErrorMessage = "Sponsor name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Logo URL cannot exceed 500 characters")]
        public string? Logo { get; set; }

        [StringLength(256, ErrorMessage = "Website URL cannot exceed 256 characters")]
        public string? Website { get; set; }
    }

    public class UpdateSponsorDto
    {
        [Required(ErrorMessage = "Sponsor name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Logo URL cannot exceed 500 characters")]
        public string? Logo { get; set; }

        [StringLength(256, ErrorMessage = "Website URL cannot exceed 256 characters")]
        public string? Website { get; set; }
    }

    public class PaginatedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}