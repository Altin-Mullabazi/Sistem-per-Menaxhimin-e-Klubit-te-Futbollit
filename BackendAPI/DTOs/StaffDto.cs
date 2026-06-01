using System;

namespace FootballClubAPI.DTOs
{
    public class StaffDto
    {
        public int Id { get; set; }
        public int? ClubId { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public DateTime EmploymentDate { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
