using FootballClubAPI.DTOs;
using Microsoft.AspNetCore.Identity;

namespace FootballClubAPI.Services
{
    public interface IUserRoleService
    {
        Task<UserRolesResponseDto?> GetUserRolesAsync(string userId);

        Task<(IdentityResult Result, UserRoleAssignmentResponseDto? Assignment)> AssignRoleAsync(string userId, string roleId);
    }
}