using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _8_ball_pool.Data;
using _8_ball_pool.Models;
using _8_ball_pool.DTOs.Player;
using _8_ball_pool.Services;
using _8_ball_pool.DTOs.S3;

namespace _8_ball_pool.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly AppDbContext _context;

    public PlayerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
    public async Task<ActionResult> CreatePlayer([FromBody] CreatePlayerDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var player = new Player
            {
                Name = dto.Name,
                Ranking = dto.Ranking,
                PreferredCue = dto.PreferredCue,
                ProfilePictureUrl = dto.ProfilePictureUrl
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

        var responseDto = MapToDto(player);
        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, responseDto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayers()
        {
            var players = await _context.Players.ToListAsync();
            return players.Select(p => MapToDto(p)).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerDto>> GetPlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null) return NotFound();

            return MapToDto(player);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlayer(int id, UpdatePlayerDto dto)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null) return NotFound();

            player.Name = dto.Name;
            player.Ranking = dto.Ranking;
            player.PreferredCue = dto.PreferredCue;
            player.ProfilePictureUrl = dto.ProfilePictureUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null) return NotFound();

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private PlayerDto MapToDto(Player player)
        {
            return new PlayerDto
            {
                Id = player.Id,
                Name = player.Name,
                Ranking = player.Ranking,
                PreferredCue = player.PreferredCue,
                ProfilePictureUrl = player.ProfilePictureUrl
            };
        }
    }
}