namespace FootballClubAPI.Helpers
{
    public static class RoleConstants
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Coach = "Coach";
        public const string User = "User";

        public static readonly string[] BuiltInRoles = [Admin, Manager, Coach, User];

        public static bool IsBuiltIn(string? roleName)
        {
            return !string.IsNullOrWhiteSpace(roleName) &&
                   BuiltInRoles.Any(role => string.Equals(role, roleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}