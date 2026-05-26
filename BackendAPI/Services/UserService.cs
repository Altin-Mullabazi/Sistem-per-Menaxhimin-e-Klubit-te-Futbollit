using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public interface IUserService
    {
        Task<PaginatedResponse<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<UserDto?> GetUserByIdAsync(string id);
        Task<(IdentityResult Result, UserDto? User)> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default);
        Task<(IdentityResult Result, UserDto? User)> UpdateUserAsync(string id, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default);
        Task<IdentityResult> DeleteUserAsync(string id, CancellationToken cancellationToken = default);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<PaginatedResponse<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            var query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim().ToLowerInvariant();
                query = query.Where(user =>
                    (user.Email ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                    (user.FirstName ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                    (user.LastName ?? string.Empty).ToLower().Contains(normalizedSearch));
            }

            query = query.OrderByDescending(user => user.CreatedAt);

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userIds = users.Select(user => user.Id).ToList();
            var roleLookup = await (from userRole in _context.UserRoles
                                    join role in _context.Roles on userRole.RoleId equals role.Id
                                    where userIds.Contains(userRole.UserId)
                                    select new { userRole.UserId, role.Name })
                .ToListAsync();
            var rolesByUserId = roleLookup
                .GroupBy(item => item.UserId)
                .ToDictionary(group => group.Key, group => group.First().Name ?? string.Empty);

            return new PaginatedResponse<UserDto>
            {
                Items = users.Select(user => MapToDto(user, rolesByUserId.TryGetValue(user.Id, out var role) ? role : user.Role)).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id);
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? user.Role;
            return MapToDto(user, role);
        }

        public async Task<(IdentityResult Result, UserDto? User)> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = createUserDto.Email.Trim().ToLowerInvariant();
            var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingUser != null)
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateEmail",
                    Description = "Email already exists"
                }), null);
            }

            var requestedRole = createUserDto.Role.Trim();

            if (!await _roleManager.RoleExistsAsync(requestedRole))
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidRole",
                    Description = $"Role '{createUserDto.Role}' is invalid"
                }), null);
            }

            var user = new ApplicationUser
            {
                Email = normalizedEmail,
                UserName = normalizedEmail,
                FirstName = createUserDto.FirstName.Trim(),
                LastName = createUserDto.LastName.Trim(),
                FullName = $"{createUserDto.FirstName.Trim()} {createUserDto.LastName.Trim()}",
                Role = requestedRole,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user, createUserDto.Password);
            if (!createResult.Succeeded)
            {
                return (createResult, null);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, requestedRole);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return (IdentityResult.Failed(roleResult.Errors.ToArray()), null);
            }

            var createdUser = await GetUserByIdAsync(user.Id);
            return (IdentityResult.Success, createdUser);
        }

        public async Task<(IdentityResult Result, UserDto? User)> UpdateUserAsync(string id, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "User not found"
                }), null);
            }

            var normalizedEmail = updateUserDto.Email.Trim().ToLowerInvariant();
            var duplicateEmailUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (duplicateEmailUser != null && duplicateEmailUser.Id != user.Id)
            {
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateEmail",
                    Description = "Email already exists"
                }), null);
            }

            user.Email = normalizedEmail;
            user.UserName = normalizedEmail;
            user.FirstName = updateUserDto.FirstName.Trim();
            user.LastName = updateUserDto.LastName.Trim();
            user.FullName = $"{updateUserDto.FirstName.Trim()} {updateUserDto.LastName.Trim()}";
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return (updateResult, null);
            }

            var updatedUser = await GetUserByIdAsync(user.Id);
            return (IdentityResult.Success, updatedUser);
        }

        public async Task<IdentityResult> DeleteUserAsync(string id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "User not found"
                });
            }

            return await _userManager.DeleteAsync(user);
        }

        private static UserDto MapToDto(ApplicationUser user, string role)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.UserName ?? user.Email ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = role,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}