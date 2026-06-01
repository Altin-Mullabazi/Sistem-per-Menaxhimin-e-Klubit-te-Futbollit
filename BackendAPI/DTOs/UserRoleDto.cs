using System.ComponentModel.DataAnnotations;

namespace FootballClubAPI.DTOs
{
    /// <summary>
    /// Request model used to assign an existing role to a user.
    /// </summary>
    public class UserRoleAssignmentDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string RoleId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Role information returned when looking up a user's assigned roles.
    /// </summary>
    public class UserRoleInfoDto
    {
        public string RoleId { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response returned by the user-role endpoints.
    /// </summary>
    public class UserRolesResponseDto
    {
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public IReadOnlyList<string> Roles { get; set; } = [];
    }

    /// <summary>
    /// Response returned after successfully assigning a role.
    /// </summary>
    public class UserRoleAssignmentResponseDto
    {
        public string UserId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string RoleId { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}