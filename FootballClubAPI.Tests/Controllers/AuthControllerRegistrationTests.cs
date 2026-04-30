using FluentValidation;
using Moq;
using Xunit;
using FootballClubAPI.Controllers;
using FootballClubAPI.Data;
using FootballClubAPI.DTOs;
using FootballClubAPI.Helpers;
using FootballClubAPI.Models;
using FootballClubAPI.Services;
using FootballClubAPI.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FootballClubAPI.Tests.Controllers
{
    public class AuthControllerRegistrationTests
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenHelper _tokenHelper;
        private readonly Mock<ILogger<AuthService>> _authServiceLoggerMock;
        private readonly Mock<ILogger<AuthController>> _controllerLoggerMock;
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterRequest> _validator;
        private readonly AuthController _controller;

        public AuthControllerRegistrationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"AuthControllerTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);

            // Setup TokenHelper
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["JwtSettings:SecretKey"]).Returns("this_is_a_secret_key_for_testing_1234567890");
            configMock.Setup(c => c["JwtSettings:Issuer"]).Returns("TestIssuer");
            configMock.Setup(c => c["JwtSettings:Audience"]).Returns("TestAudience");
            _tokenHelper = new TokenHelper(configMock.Object);

            _authServiceLoggerMock = new Mock<ILogger<AuthService>>();
            _authService = new AuthService(_context, _tokenHelper, _authServiceLoggerMock.Object);

            _validator = new RegisterRequestValidator();
            _controllerLoggerMock = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_authService, _validator, _controllerLoggerMock.Object);
        }

        /// <summary>
        /// Test: Valid registration returns 201 Created status
        /// </summary>
        [Fact]
        public async Task Register_WithValidRequest_Returns201Created()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "valid@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
            var response = result.Value as RegisterResponse;
            Assert.NotNull(response);
            Assert.True(response.Success);
        }

        /// <summary>
        /// Test: Missing email returns 400 Bad Request
        /// </summary>
        [Fact]
        public async Task Register_WithMissingEmail_Returns400BadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "", // Missing email
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            var response = result.Value as RegisterResponse;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.NotNull(response.Errors);
            Assert.Contains(response.Errors, e => e.Field == "Email");
        }

        /// <summary>
        /// Test: Invalid email format returns 400 Bad Request
        /// </summary>
        [Fact]
        public async Task Register_WithInvalidEmailFormat_Returns400BadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "invalidemail",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Test: Weak password returns 400 Bad Request
        /// </summary>
        [Fact]
        public async Task Register_WithWeakPassword_Returns400BadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "weak", // Too short and no complexity
                ConfirmPassword = "weak",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            var response = result.Value as RegisterResponse;
            Assert.NotNull(response);
            Assert.False(response.Success);
        }

        /// <summary>
        /// Test: Mismatched passwords returns 400 Bad Request
        /// </summary>
        [Fact]
        public async Task Register_WithMismatchedPasswords_Returns400BadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "DifferentPass123!",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Test: Duplicate email returns 409 Conflict
        /// </summary>
        [Fact]
        public async Task Register_WithDuplicateEmail_Returns409Conflict()
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
            var firstResult = await _controller.Register(firstRequest);
            var secondResult = await _controller.Register(secondRequest) as ObjectResult;

            // Assert
            Assert.NotNull(secondResult);
            Assert.Equal(StatusCodes.Status409Conflict, secondResult.StatusCode);
        }

        /// <summary>
        /// Test: Missing FirstName returns 400 Bad Request
        /// </summary>
        [Fact]
        public async Task Register_WithMissingFirstName_Returns400BadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "", // Missing
                LastName = "Doe"
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Test: Missing LastName returns 400 Bad Request
        /// </summary>
        [Fact]
        public async Task Register_WithMissingLastName_Returns400BadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "John",
                LastName = "" // Missing
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Test: FirstName too short returns 400 Bad Request
        /// </summary>
        [Fact]
        public async Task Register_WithFirstNameTooShort_Returns400BadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "A", // Too short
                LastName = "Doe"
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Test: LastName with special characters returns 400 Bad Request
        /// </summary>
        [Fact]
        public async Task Register_WithLastNameSpecialChars_Returns400BadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "John",
                LastName = "Doe@123" // Invalid characters
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Test: Valid registration response includes user data and tokens
        /// </summary>
        [Fact]
        public async Task Register_SuccessfulRegistration_IncludesUserDataAndTokens()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "complete@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "Complete",
                LastName = "Test"
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;
            var response = result?.Value as RegisterResponse;

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("complete@example.com", response.Data.Email);
            Assert.Equal("Complete", response.Data.FirstName);
            Assert.Equal("Test", response.Data.LastName);
            Assert.Equal("Complete Test", response.Data.FullName);
            Assert.NotNull(response.Data.Tokens);
            Assert.NotEmpty(response.Data.Tokens.AccessToken);
            Assert.NotEmpty(response.Data.Tokens.RefreshToken);
            Assert.Equal(3600, response.Data.Tokens.ExpiresIn);
        }

        /// <summary>
        /// Test: Empty FirstName returns 400 Bad Request
        /// </summary>
        [Fact]
        public async Task Register_WithEmptyFirstName_Returns400BadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!",
                FirstName = "   ", // Whitespace only
                LastName = "Doe"
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        /// <summary>
        /// Test: Response contains validation errors with field and message
        /// </summary>
        [Fact]
        public async Task Register_WithValidationErrors_IncludesFieldAndMessage()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "invalidemail",
                Password = "weak",
                ConfirmPassword = "different",
                FirstName = "A",
                LastName = ""
            };

            // Act
            var result = await _controller.Register(request) as ObjectResult;
            var response = result?.Value as RegisterResponse;

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.NotNull(response.Errors);
            Assert.NotEmpty(response.Errors);
            Assert.True(response.Errors.All(e => !string.IsNullOrEmpty(e.Field)));
            Assert.True(response.Errors.All(e => !string.IsNullOrEmpty(e.Message)));
        }
    }
}
