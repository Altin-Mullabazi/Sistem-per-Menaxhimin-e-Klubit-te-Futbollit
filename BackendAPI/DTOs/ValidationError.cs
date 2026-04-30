namespace FootballClubAPI.DTOs
{
    /// <summary>
    /// Represents a validation error for a specific field.
    /// </summary>
    public class ValidationError
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
