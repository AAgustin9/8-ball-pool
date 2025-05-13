using System.ComponentModel.DataAnnotations;

namespace _8_ball_pool.DTOs.Player
{
    public class UpdatePlayerDto
    {
        [Required]
        public required string Name { get; set; }

        public int Ranking { get; set; }

        public string? PreferredCue { get; set; }

        [Required]
        public required string ProfilePictureUrl { get; set; }
    }
}