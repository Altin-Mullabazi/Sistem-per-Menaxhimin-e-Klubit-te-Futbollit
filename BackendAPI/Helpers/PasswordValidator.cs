using System.Text.RegularExpressions;

namespace FootballClubAPI.Helpers
{
    /// <summary>
    /// Utility class for validating password complexity requirements.
    /// Password must contain:
    /// - Minimum 8 characters
    /// - At least 1 uppercase letter
    /// - At least 1 lowercase letter
    /// - At least 1 digit
    /// - At least 1 special character (!@#$%^&*)
    /// </summary>
    public static class PasswordValidator
    {
        private const int MinimumLength = 8;
        private const string SpecialCharacters = "!@#$%^&*";

        /// <summary>
        /// Validates password complexity based on requirements.
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <returns>A tuple containing validation status and error message if invalid</returns>
        public static (bool isValid, string? errorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot be empty");

            if (password.Length < MinimumLength)
                return (false, $"Password must be at least {MinimumLength} characters");

            if (!Regex.IsMatch(password, "[A-Z]"))
                return (false, "Password must contain at least one uppercase letter");

            if (!Regex.IsMatch(password, "[a-z]"))
                return (false, "Password must contain at least one lowercase letter");

            if (!Regex.IsMatch(password, "[0-9]"))
                return (false, "Password must contain at least one digit");

            if (!password.Any(c => SpecialCharacters.Contains(c)))
                return (false, $"Password must contain at least one special character ({SpecialCharacters})");

            return (true, null);
        }
    }
}
