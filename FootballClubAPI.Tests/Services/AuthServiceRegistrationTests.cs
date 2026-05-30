using Moq;
using Xunit;
using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Helpers;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FootballClubAPI.Tests.Services
{
    public class AuthServiceRegistrationTests
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenHelper _tokenHelper;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceRegistrationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"AuthServiceTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);

            // Mock TokenHelper
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["JwtSettings:SecretKey"]).Returns("this_is_a_secret_key_for_testing_1234567890");
            configMock.Setup(c => c["JwtSettings:Issuer"]).Returns("TestIssuer");
            configMock.Setup(c => c["JwtSettings:Audience"]).Returns("TestAudience");
            _tokenHelper = new TokenHelper(configMock.Object);

            _loggerMock = new Mock<ILogger<AuthService>>();
            SeedRoles();
            _authService = new AuthService(_context, _tokenHelper, _loggerMock.Object);
        }

        private void SeedRoles()
        {
            foreach (var roleName in new[] { "Admin", "Manager", "Coach", "User" })
            {
                if (!_context.Roles.Any(role => role.Name == roleName))
                {
                    _context.Roles.Add(new IdentityRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpperInvariant()
                    });
                }
            }

            _context.SaveChanges();
        }

        /// <summary>
        /// Test: Valid registration returns success with user data and tokens
        /// </summary>
        [Fact]
        public async Task RegisterAsync_WithValidRequest_ReturnsSuccessWithUserData()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("newuser@example.com", result.Data.Email);
            Assert.Equal("John", result.Data.FirstName);
            Assert.Equal("Doe", result.Data.LastName);
            Assert.Equal("John Doe", result.Data.FullName);
            Assert.NotNull(result.Data.Tokens);
            Assert.NotEmpty(result.Data.Tokens.AccessToken);
            Assert.NotEmpty(result.Data.Tokens.RefreshToken);
            Assert.Equal(3600, result.Data.Tokens.ExpiresIn);
        }

        /// <summary>
        /// Test: User is created in database after registration
        /// </summary>
        [Fact]
        public async Task RegisterAsync_WithValidRequest_CreatesUserInDatabase()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "dbtest@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "Jane",
                LastName = "Smith"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.True(result.Success);
            var userInDb = _context.Users.FirstOrDefault(u => u.Email == "dbtest@example.com");
            Assert.NotNull(userInDb);
            Assert.Equal("Jane", userInDb.FirstName);
            Assert.Equal("Smith", userInDb.LastName);
        }

        /// <summary>
        /// Test: Password is hashed using BCrypt, not stored plaintext
        /// </summary>
        [Fact]
        public async Task RegisterAsync_PasswordIsHashedNotPlaintext()
        {
            // Arrange
            var plainPassword = "SecurePass123!";
            var request = new RegisterRequest
            {
                Email = "hashtest@example.com",
                Password = plainPassword,
                ConfirmPassword = plainPassword,
                FirstName = "Test",
                LastName = "User"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            var userInDb = _context.Users.FirstOrDefault(u => u.Email == "hashtest@example.com");
            Assert.NotNull(userInDb);
            Assert.NotEqual(plainPassword, userInDb.PasswordHash);

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var verifyResult = passwordHasher.VerifyHashedPassword(userInDb!, userInDb.PasswordHash, plainPassword);
            Assert.Equal(PasswordVerificationResult.Success, verifyResult);
        }

        /// <summary>
        /// Test: JWT tokens are issued after registration
        /// </summary>
        [Fact]
        public async Task RegisterAsync_TokensAreIssuedAfterRegistration()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "tokentest@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "Token",
                LastName = "User"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data?.Tokens);
            Assert.NotEmpty(result.Data.Tokens.AccessToken);
            Assert.NotEmpty(result.Data.Tokens.RefreshToken);
            
            // Verify refresh token is stored in database
            var refreshTokensInDb = _context.RefreshTokens
                .Where(rt => rt.UserId == result.Data.UserId && !rt.IsRevoked)
                .ToList();
            Assert.Single(refreshTokensInDb);
        }

        /// <summary>
        /// Test: Duplicate email returns error with 409 status code
        /// </summary>
        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ReturnsFalseWithMessage()
        {
            // Arrange
            var firstRequest = new RegisterRequest
            {
                Email = "duplicate@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "First",
                LastName = "User"
            };

            var secondRequest = new RegisterRequest
            {
                Email = "duplicate@example.com",
                Password = "AnotherPass123!",
                ConfirmPassword = "AnotherPass123!",
                FirstName = "Second",
                LastName = "User"
            };

            // Act
            var firstResult = await _authService.RegisterAsync(firstRequest);
            var secondResult = await _authService.RegisterAsync(secondRequest);

            // Assert
            Assert.True(firstResult.Success);
            Assert.False(secondResult.Success);
            Assert.Contains("already registered", secondResult.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Test: Email is normalized (trimmed and lowercase)
        /// </summary>
        [Fact]
        public async Task RegisterAsync_EmailIsNormalized()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "  USER@EXAMPLE.COM  ",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "Email",
                LastName = "Test"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("user@example.com", result.Data?.Email);
            var userInDb = _context.Users.FirstOrDefault(u => u.Email == "user@example.com");
            Assert.NotNull(userInDb);
        }

        /// <summary>
        /// Test: Default role is "User" for new registrations
        /// </summary>
        [Fact]
        public async Task RegisterAsync_DefaultRoleIsUser()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "roletest@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "Role",
                LastName = "Test"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            var userInDb = _context.Users.FirstOrDefault(u => u.Email == "roletest@example.com");
            Assert.NotNull(userInDb);
            Assert.Equal("User", userInDb.Role);
        }

        /// <summary>
        /// Test: User email is verified as false by default
        /// </summary>
        [Fact]
        public async Task RegisterAsync_EmailNotVerifiedByDefault()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "emailverify@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "Verify",
                LastName = "Test"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            var userInDb = _context.Users.FirstOrDefault(u => u.Email == "emailverify@example.com");
            Assert.NotNull(userInDb);
            Assert.False(userInDb.EmailConfirmed);
        }

        /// <summary>
        /// Test: User is active by default
        /// </summary>
        [Fact]
        public async Task RegisterAsync_UserIsActiveByDefault()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "activetest@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "Active",
                LastName = "Test"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            var userInDb = _context.Users.FirstOrDefault(u => u.Email == "activetest@example.com");
            Assert.NotNull(userInDb);
            Assert.True(userInDb.IsActive);
        }

        /// <summary>
        /// Test: CreatedAt timestamp is set
        /// </summary>
        [Fact]
        public async Task RegisterAsync_CreatedAtTimestampIsSet()
        {
            // Arrange
            var beforeRegistration = DateTime.UtcNow;
            var request = new RegisterRequest
            {
                Email = "timestamptest@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "Timestamp",
                LastName = "Test"
            };

            // Act
            var result = await _authService.RegisterAsync(request);
            var afterRegistration = DateTime.UtcNow;

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data!.CreatedAt >= beforeRegistration && result.Data.CreatedAt <= afterRegistration);
        }

        /// <summary>
        /// Test: Newly registered user can login with credentials
        /// </summary>
        [Fact]
        public async Task RegisterAsync_UserCanLoginAfterRegistration()
        {
            // Arrange
            var password = "SecurePass123!";
            var registerRequest = new RegisterRequest
            {
                Email = "logintest@example.com",
                Password = password,
                ConfirmPassword = password,
                FirstName = "Login",
                LastName = "Test"
            };

            var loginRequest = new LoginDto
            {
                Email = "logintest@example.com",
                Password = password
            };

            // Act
            var registerResult = await _authService.RegisterAsync(registerRequest);
            var loginResult = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.True(registerResult.Success);
            Assert.True(loginResult.Success);
            Assert.NotNull(loginResult.AccessToken);
            Assert.NotNull(loginResult.User);
        }
    }
}
