using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.DTOs
{
    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool IsBuiltIn { get; set; }
    }

    public class CreateRoleDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateRoleDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }
}