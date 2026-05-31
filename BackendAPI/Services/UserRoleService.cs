using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserRoleService> _logger;

        public UserRoleService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserRoleService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Returns the user together with every role currently assigned to that account.
        /// </summary>
        public async Task<UserRolesResponseDto?> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(currentUser => currentUser.Id == userId);
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserRolesResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? user.Email ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.OrderBy(role => role).ToList()
            };
        }

        /// <summary>
        /// Assigns a role to a user after validating the user, the role, and duplicate membership.
        /// </summary>
        public async Task<(IdentityResult Result, UserRoleAssignmentResponseDto? Assignment)> AssignRoleAsync(string userId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "User not found"
                }), null);
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidRole",
                    Description = "Role not found"
                }), null);
            }

            if (await _userManager.IsInRoleAsync(user, role.Name))
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateRoleAssignment",
                    Description = "This role is already assigned to the user"
                }), null);
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (!result.Succeeded)
            {
                return (result, null);
            }

            _logger.LogInformation("Assigned role {RoleName} to user {UserId}", role.Name, user.Id);

            return (IdentityResult.Success, new UserRoleAssignmentResponseDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                RoleId = role.Id,
                RoleName = role.Name,
                Message = $"Role '{role.Name}' assigned successfully"
            });
        }
    }
}