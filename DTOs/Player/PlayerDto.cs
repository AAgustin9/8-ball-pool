namespace _8_ball_pool.DTOs.Player
{
    public class PlayerDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int Ranking { get; set; }
        public string? PreferredCue { get; set; }
        public required string ProfilePictureUrl { get; set; }
    }
}