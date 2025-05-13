using System.ComponentModel.DataAnnotations;

namespace _8_ball_pool.DTOs.Match
{
    public class CreateMatchDto
    {
        [Required]
        public int Player1Id { get; set; }

        [Required]
        public int Player2Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public int? TableNumber { get; set; }
    }
}