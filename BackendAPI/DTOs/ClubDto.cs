namespace FootballClubAPI.DTOs
{
    public class ClubDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public int FoundedYear { get; set; }
        public string? President { get; set; }
        public decimal? Budget { get; set; }
        public int PlayerCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ClubDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public int FoundedYear { get; set; }
        public string? President { get; set; }
        public decimal? Budget { get; set; }
        public int PlayerCount { get; set; }
        public List<ClubPlayerSummaryDto> Players { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ClubPlayerSummaryDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public int JerseyNumber { get; set; }
    }

    public class ClubListResponseDto
    {
        public List<ClubDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class CreateClubDto
    {
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public int FoundedYear { get; set; }
        public string? President { get; set; }
        public decimal? Budget { get; set; }
    }

    public class UpdateClubDto
    {
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? LogoUrl { get; set; }
        public int? FoundedYear { get; set; }
        public string? President { get; set; }
        public decimal? Budget { get; set; }
    }
}
