using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.DTOs
{
    public class PlayerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int JerseyNumber { get; set; }
        public string Position { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Status { get; set; }
        public decimal? MarketValue { get; set; }
        public int? ClubId { get; set; }
        public ClubDto? Club { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PlayerDetailDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int JerseyNumber { get; set; }
        public string Position { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Status { get; set; }
        public decimal? MarketValue { get; set; }
        public int? ClubId { get; set; }
        public ClubDto? Club { get; set; }
        public List<ContractDto> Contracts { get; set; } = new();
        public List<TransferDto> Transfers { get; set; } = new();
        public PlayerStatsDto? Stats { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreatePlayerDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Range(1, 99, ErrorMessage = "Jersey number must be between 1 and 99")]
        public int JerseyNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Position { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [StringLength(50)]
        public string Nationality { get; set; } = string.Empty;

        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public decimal? MarketValue { get; set; }

        [Required]
        public int? ClubId { get; set; }
    }

    public class UpdatePlayerDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Range(1, 99, ErrorMessage = "Jersey number must be between 1 and 99")]
        public int JerseyNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Position { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [StringLength(50)]
        public string Nationality { get; set; } = string.Empty;

        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public decimal? MarketValue { get; set; }

        [Required]
        public int? ClubId { get; set; }
    }
}
