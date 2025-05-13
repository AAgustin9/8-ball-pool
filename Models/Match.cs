using System.ComponentModel.DataAnnotations;

namespace _8_ball_pool.Models
{
    public class Match
    {
        public int Id { get; set; }

        [Required]
        public int Player1Id { get; set; }

        [Required]
        public int Player2Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? WinnerId { get; set; }

        public int? TableNumber { get; set; }

        // Navigation
        public Player Player1 { get; set; } = null!;
        public Player Player2 { get; set; } = null!;
        public Player? Winner { get; set; }
    }
}
