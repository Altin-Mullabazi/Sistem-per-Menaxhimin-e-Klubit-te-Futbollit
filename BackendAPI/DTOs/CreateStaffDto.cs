using System;
using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.DTOs
{
    public class CreateStaffDto
    {
        [Required]
        public int ClubId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Role { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Specialization { get; set; }

        public DateTime EmploymentDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? Status { get; set; }
    }
}
