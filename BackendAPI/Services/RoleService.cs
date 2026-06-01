using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Helpers;
using FootballClubAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public interface IRoleService
    {
        Task<IReadOnlyList<RoleDto>> GetRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(string id);
        Task<(IdentityResult Result, RoleDto? Role)> CreateRoleAsync(CreateRoleDto createRoleDto);
        Task<(IdentityResult Result, RoleDto? Role)> UpdateRoleAsync(string id, UpdateRoleDto updateRoleDto);
        Task<IdentityResult> DeleteRoleAsync(string id);
    }

    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IReadOnlyList<RoleDto>> GetRolesAsync()
        {
            return await _roleManager.Roles
                .AsNoTracking()
                .OrderBy(role => role.Name)
                .Select(role => new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    IsBuiltIn = RoleConstants.IsBuiltIn(role.Name)
                })
                .ToListAsync();
        }

        public async Task<RoleDto?> GetRoleByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return null;
            }

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                IsBuiltIn = RoleConstants.IsBuiltIn(role.Name)
            };
        }

        public async Task<(IdentityResult Result, RoleDto? Role)> CreateRoleAsync(CreateRoleDto createRoleDto)
        {
            var roleName = createRoleDto.Name.Trim();

            if (string.IsNullOrWhiteSpace(roleName))
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidRoleName",
                    Description = "Role name is required"
                }), null);
            }

            if (await _roleManager.FindByNameAsync(roleName) != null)
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateRoleName",
                    Description = "Role name already exists"
                }), null);
            }

            var role = new IdentityRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return (result, null);
            }

            return (IdentityResult.Success, new RoleDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                IsBuiltIn = false
            });
        }

        public async Task<(IdentityResult Result, RoleDto? Role)> UpdateRoleAsync(string id, UpdateRoleDto updateRoleDto)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "Role not found"
                }), null);
            }

            if (RoleConstants.IsBuiltIn(role.Name))
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "BuiltInRole",
                    Description = "Built-in roles cannot be modified"
                }), null);
            }

            var roleName = updateRoleDto.Name.Trim();
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidRoleName",
                    Description = "Role name is required"
                }), null);
            }

            var existingRole = await _roleManager.FindByNameAsync(roleName);
            if (existingRole != null && existingRole.Id != role.Id)
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateRoleName",
                    Description = "Role name already exists"
                }), null);
            }

            role.Name = roleName;
            role.NormalizedName = roleName.ToUpperInvariant();

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return (result, null);
            }

            return (IdentityResult.Success, new RoleDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                IsBuiltIn = false
            });
        }

        public async Task<IdentityResult> DeleteRoleAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "Role not found"
                });
            }

            if (RoleConstants.IsBuiltIn(role.Name))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "BuiltInRole",
                    Description = "Built-in roles cannot be deleted"
                });
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);
            if (usersInRole.Count > 0)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "RoleAssignedToUsers",
                    Description = "Role cannot be deleted because users are assigned to it"
                });
            }

            return await _roleManager.DeleteAsync(role);
        }
    }
}