using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FootballClubAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Fan";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        public virtual ICollection<Club> Clubs { get; set; } = new List<Club>();
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}