using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace FootballClubAPI.Helpers
{
    /// <summary>
    /// Helper class for managing JWT tokens and password hashing.
    /// Uses BCrypt for password hashing (cost factor = 12) and SHA256 for refresh tokens.
    /// </summary>
    public class TokenHelper
    {
        private readonly IConfiguration _configuration;
        private const int BcryptWorkFactor = 12;
        private const int AccessTokenExpirationMinutes = 60; // 1 hour

        public TokenHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates a JWT access token for the specified user.
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="role">The user's role</param>
        /// <returns>A signed JWT access token</returns>
        public string GenerateAccessToken(string userId, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"] ?? ""));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role)
                },
                expires: DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a cryptographically secure refresh token.
        /// </summary>
        /// <returns>A random base64-encoded refresh token</returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Hashes a refresh token using SHA256 for storage in database.
        /// </summary>
        /// <param name="refreshToken">The refresh token to hash</param>
        /// <returns>A base64-encoded SHA256 hash</returns>
        public string HashRefreshToken(string refreshToken)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Validates a refresh token against its stored hash.
        /// </summary>
        /// <param name="refreshToken">The plaintext refresh token</param>
        /// <param name="storedHash">The stored hash to compare against</param>
        /// <returns>True if token matches hash, false otherwise</returns>
        public bool ValidateRefreshToken(string refreshToken, string storedHash)
        {
            var hash = HashRefreshToken(refreshToken);
            return hash == storedHash;
        }

        /// <summary>
        /// Hashes a password using BCrypt with configurable work factor.
        /// BCrypt automatically handles salt generation and is resistant to timing attacks.
        /// </summary>
        /// <param name="password">The plaintext password to hash</param>
        /// <returns>A BCrypt-hashed password string</returns>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BcryptWorkFactor);
        }

        /// <summary>
        /// Verifies a plaintext password against its BCrypt hash.
        /// </summary>
        /// <param name="password">The plaintext password to verify</param>
        /// <param name="hash">The BCrypt hash to verify against</param>
        /// <returns>True if password matches hash, false otherwise</returns>
        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}
