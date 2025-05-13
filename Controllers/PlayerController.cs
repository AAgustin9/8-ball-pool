using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _8_ball_pool.Data;
using _8_ball_pool.Models;
using _8_ball_pool.DTOs.Player;

[ApiController]
[Route("api/players")]
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
        return CreatedAtAction(nameof(GetPlayerById), new { id = player.Id }, responseDto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayers([FromQuery] string? name)
    {
        var query = _context.Players.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        var players = await query.ToListAsync();

        return players.Select(MapToDto).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDto>> GetPlayerById(int id)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null) return NotFound();

        return MapToDto(player);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(int id, [FromBody] UpdatePlayerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var player = await _context.Players.FindAsync(id);
        if (player == null) return NotFound();

        // Actualizamos propiedades
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

    private static PlayerDto MapToDto(Player p) => new PlayerDto
    {
        Id = p.Id,
        Name = p.Name,
        Ranking = p.Ranking,
        PreferredCue = p.PreferredCue,
        ProfilePictureUrl = p.ProfilePictureUrl
    };
}