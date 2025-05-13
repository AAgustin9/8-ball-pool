namespace _8_ball_pool.DTOs.Match
{
    public class MatchDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? TableNumber { get; set; }

        public required string Player1Name { get; set; }
        public required string Player2Name { get; set; }
        public string? WinnerName { get; set; }

        public required string Status { get; set; } // derived: "upcoming", "ongoing", "completed"
    }
}