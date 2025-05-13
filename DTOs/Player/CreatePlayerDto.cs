using System.ComponentModel.DataAnnotations;

namespace _8_ball_pool.DTOs.Player
{
    public class CreatePlayerDto
    {
        [Required]
        public required string Name { get; set; }

        public int Ranking { get; set; } = 0;

        public string? PreferredCue { get; set; }

        [Required]
        public required string ProfilePictureUrl { get; set; }
    }
}