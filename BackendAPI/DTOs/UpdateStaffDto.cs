using System;
using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.DTOs
{
    public class UpdateStaffDto
    {
        public int? ClubId { get; set; }
        public string? UserId { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(100)]
        public string? Role { get; set; }

        [StringLength(200)]
        public string? Specialization { get; set; }

        public DateTime? EmploymentDate { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }
    }
}
