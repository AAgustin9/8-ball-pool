using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace _8_ball_pool.DTOs.Player
{
    public class CreatePlayerDto
    {
        [Required]
        public required string Name { get; set; }

        public int Ranking { get; set; } = 0;

        public string? PreferredCue { get; set; }

        public string ProfilePictureUrl { get; set; }
    }
}