namespace FootballClubAPI.DTOs
{
    public class MatchEventListDto
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public int Minute { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MatchEventDetailDto
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string PlayerPosition { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public int Minute { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMatchEventDto
    {
        public int MatchId { get; set; }
        public int PlayerId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int Minute { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateMatchEventDto
    {
        public string? EventType { get; set; }
        public int? Minute { get; set; }
        public string? Description { get; set; }
    }
}
