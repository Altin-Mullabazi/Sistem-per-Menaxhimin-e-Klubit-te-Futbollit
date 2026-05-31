using Microsoft.AspNetCore.Identity;

namespace FootballClubAPI.Helpers
{
    public sealed class BcryptPasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        private const int WorkFactor = 12;

        public string HashPassword(TUser user, string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                return PasswordVerificationResult.Failed;
            }

            if (hashedPassword.StartsWith("$2", StringComparison.Ordinal))
            {
                return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword)
                    ? PasswordVerificationResult.Success
                    : PasswordVerificationResult.Failed;
            }

            var legacyResult = new PasswordHasher<TUser>().VerifyHashedPassword(user, hashedPassword, providedPassword);
            return legacyResult;
        }
    }
}