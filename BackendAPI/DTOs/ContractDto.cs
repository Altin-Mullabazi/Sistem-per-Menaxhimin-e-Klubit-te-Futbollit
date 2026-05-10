using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.DTOs
{
    public class ContractDto
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int ClubId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Salary { get; set; }
        public string Position { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public PlayerDto? Player { get; set; }
        public ClubDto? Club { get; set; }
    }

    public class CreateContractDto
    {
        [Required(ErrorMessage = "Player ID is required")]
        public int PlayerId { get; set; }

        [Required(ErrorMessage = "Club ID is required")]
        public int ClubId { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than 0")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [StringLength(50, ErrorMessage = "Position cannot exceed 50 characters")]
        public string Position { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateContractDto
    {
        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than 0")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [StringLength(50, ErrorMessage = "Position cannot exceed 50 characters")]
        public string Position { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    public class ContractQueryParameters
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        public int? PlayerId { get; set; }
        public bool? IsActive { get; set; }

        [Range(1, 365, ErrorMessage = "Days must be between 1 and 365")]
        public int? Days { get; set; } // For expiring contracts
    }

    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}