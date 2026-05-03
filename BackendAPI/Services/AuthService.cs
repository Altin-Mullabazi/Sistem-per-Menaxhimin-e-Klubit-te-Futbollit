using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Helpers;
using FootballClubAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FootballClubAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<bool> LogoutAsync(string userId);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
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
                var normalizedEmail = loginDto.Email.Trim().ToLowerInvariant();

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
                _logger.LogError(ex, "Login error");
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
                _logger.LogError(ex, "Token refresh error");
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
                _logger.LogError(ex, "Logout error");
                return false;
            }
        }

        /// <summary>
        /// Registers a new user account with the provided credentials and information.
        /// Password is hashed using BCrypt before storage. JWT tokens are issued immediately after successful registration.
        /// </summary>
        /// <param name="request">The registration request containing user data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Registration response with user data and tokens if successful, or errors if registration fails</returns>
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            IDbContextTransaction? transaction = null;

            try
            {
                // Normalize email
                var normalizedEmail = request.Email.Trim().ToLowerInvariant();

                // Check if email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt with duplicate email: {Email}", normalizedEmail);
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Email already registered. Please use a different email or try login."
                    };
                }

                // Hash password using BCrypt
                var passwordHash = _tokenHelper.HashPassword(request.Password);

                // Create new user
                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = normalizedEmail,
                    Username = normalizedEmail, // Use email as username initially
                    PasswordHash = passwordHash,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Role = "Fan", // Default role for new registrations
                    EmailVerified = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Generate JWT tokens
                var accessToken = _tokenHelper.GenerateAccessToken(newUser.Id, newUser.Role);
                var refreshToken = _tokenHelper.GenerateRefreshToken();

                // Store refresh token in database
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = newUser.Id,
                    TokenHash = _tokenHelper.HashRefreshToken(refreshToken),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                };

                if (_context.Database.IsRelational())
                {
                    transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                }

                _context.Users.Add(newUser);
                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync(cancellationToken);
                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                _logger.LogInformation("User registered successfully: {Email}", normalizedEmail);

                // Build response
                var response = new RegisterResponse
                {
                    Success = true,
                    Message = "User registered successfully. Please check your email to verify your account.",
                    Data = new RegisterResponseData
                    {
                        UserId = newUser.Id,
                        Email = newUser.Email,
                        FirstName = newUser.FirstName,
                        LastName = newUser.LastName,
                        FullName = newUser.FullName,
                        CreatedAt = newUser.CreatedAt,
                        Tokens = new TokenData
                        {
                            AccessToken = accessToken,
                            RefreshToken = refreshToken,
                            ExpiresIn = 3600 // 1 hour in seconds
                        }
                    }
                };

                return response;
            }
            catch (DbUpdateException ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                _logger.LogWarning(ex, "Registration failed due to database constraint violation");
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Email already registered. Please use a different email or try login."
                };
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                _logger.LogError(ex, "Registration error");
                return new RegisterResponse
                {
                    Success = false,
                    Message = "An error occurred during registration. Please try again later."
                };
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

    }
}
