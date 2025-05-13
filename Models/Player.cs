using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace _8_ball_pool.Models
{
    public class Player
    {
        [Required]
        public int Id { get; set; } // or Guid Id

        public required string Name { get; set; }

        public int Ranking { get; set; } = 0;

        public string? PreferredCue { get; set; }

        [Required]
        public required string ProfilePictureUrl { get; set; }
        
        // Navigation properties for relationships
        public virtual ICollection<Match> MatchesAsPlayer1 { get; set; } = new List<Match>();
        public virtual ICollection<Match> MatchesAsPlayer2 { get; set; } = new List<Match>();
    }
}