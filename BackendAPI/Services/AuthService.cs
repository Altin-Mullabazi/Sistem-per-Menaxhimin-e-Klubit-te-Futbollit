using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Helpers;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballClubAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<bool> LogoutAsync(string userId);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenHelper _tokenHelper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, TokenHelper tokenHelper, ILogger<AuthService> logger)
        {
            _context = context;
            _tokenHelper = tokenHelper;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var normalizedEmail = loginDto.Email.Trim();

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

                if (user == null || !_tokenHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                if (!user.IsActive)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User account is inactive"
                    };
                }

                var accessToken = _tokenHelper.GenerateAccessToken(user.Id, user.Role);
                var refreshToken = _tokenHelper.GenerateRefreshToken();

              
                var oldTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var oldToken in oldTokens)
                {
                    oldToken.IsRevoked = true;
                    oldToken.RevokedAt = DateTime.UtcNow;
                }

                
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = _tokenHelper.HashRefreshToken(refreshToken),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var tokenHash = _tokenHelper.HashRefreshToken(refreshTokenDto.RefreshToken);

                var refreshTokenEntity = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => 
                        rt.TokenHash == tokenHash && 
                        !rt.IsRevoked && 
                        rt.ExpiresAt > DateTime.UtcNow);

                if (refreshTokenEntity == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    };
                }

                var user = await _context.Users.FindAsync(refreshTokenEntity.UserId);
                if (user == null || !user.IsActive)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User not found or inactive"
                    };
                }

                
                refreshTokenEntity.IsRevoked = true;
                refreshTokenEntity.RevokedAt = DateTime.UtcNow;

                var newAccessToken = _tokenHelper.GenerateAccessToken(user.Id, user.Role);
                var newRefreshToken = _tokenHelper.GenerateRefreshToken();

                var newRefreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = _tokenHelper.HashRefreshToken(newRefreshToken),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                };

                _context.RefreshTokens.Update(refreshTokenEntity);
                _context.RefreshTokens.Add(newRefreshTokenEntity);
                await _context.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token refresh error: {ex.Message}");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during token refresh"
                };
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            try
            {
                var userTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId)
                    .ToListAsync();

                if (userTokens.Count == 0)
                {
                    return true;
                }

                _context.RefreshTokens.RemoveRange(userTokens);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Logout error: {ex.Message}");
                return false;
            }
        }

    }
}
