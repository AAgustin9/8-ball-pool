
namespace _8_ball_pool.DTOs.Match
{
    public class UpdateMatchDto
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? WinnerId { get; set; }
        public int? TableNumber { get; set; }
    }
}