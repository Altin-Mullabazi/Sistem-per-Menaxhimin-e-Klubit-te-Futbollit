namespace FootballClubAPI.DTOs
{
    /// <summary>
    /// Data transfer object for registration response containing user data and tokens.
    /// </summary>
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public RegisterResponseData? Data { get; set; }
        public List<ValidationError>? Errors { get; set; }
    }

    /// <summary>
    /// User data included in registration response.
    /// </summary>
    public class RegisterResponseData
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public TokenData? Tokens { get; set; }
    }

    /// <summary>
    /// Token data including access token, refresh token, and expiration time.
    /// </summary>
    public class TokenData
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; } // In seconds
    }
}
