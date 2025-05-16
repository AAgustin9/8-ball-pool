using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _8_ball_pool.Models
{
    public class Match
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Player1")]
        public int Player1Id { get; set; }

        [Required]
        [ForeignKey("Player2")]
        public int Player2Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [ForeignKey("Winner")]
        public int? WinnerId { get; set; }

        public int? TableNumber { get; set; }

        // Navigation properties
        public virtual Player Player1 { get; set; } = null!;
        public virtual Player Player2 { get; set; } = null!;
        public virtual Player? Winner { get; set; }
    }
}
