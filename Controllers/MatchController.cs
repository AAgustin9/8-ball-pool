using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _8_ball_pool.Data;
using _8_ball_pool.Models;
using _8_ball_pool.DTOs.Match;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly AppDbContext _context;

    public MatchesController(AppDbContext context)
    {
        _context = context;
    }


    [HttpPost]
    public async Task<ActionResult> CreateMatch([FromBody] CreateMatchDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var endTime = dto.StartTime.AddHours(1); // Asumiendo q x default dura 1h

        if (await HasDoubleBooking(dto.Player1Id, dto.StartTime, endTime) ||
            await HasDoubleBooking(dto.Player2Id, dto.StartTime, endTime))
        {
            return Conflict("One of the players has another match at this time.");
        }

        var match = new Match
        {
            Player1Id = dto.Player1Id,
            Player2Id = dto.Player2Id,
            StartTime = dto.StartTime,
            TableNumber = dto.TableNumber,
        };

        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMatchById), new { id = match.Id }, match);
    }

    // /matches?date=yyyy-mm-dd&status=ongoing|completed|upcoming
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatches([FromQuery] DateTime? date, [FromQuery] string? status)
    {
        var query = _context.Matches
        .Include(m => m.Player1)
        .Include(m => m.Player2)
        .Include(m => m.Winner)
        .AsQueryable();

        if (date.HasValue)
        {
            query = query.Where(m => m.StartTime.Date == date.Value.Date);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = status switch
            {
                "upcoming" => query.Where(m => m.StartTime > DateTime.UtcNow),
                "ongoing" => query.Where(m => m.StartTime <= DateTime.UtcNow && m.EndTime == null),
                "completed" => query.Where(m => m.EndTime != null),
                _ => query
            };
        }

        var matches = await query.ToListAsync();
        return Ok(matches.Select(MapToDto));
        
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MatchDto>> GetMatchById(int id)
    {
        var match = await _context.Matches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Winner)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (match == null) return NotFound();

        return MapToDto(match);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMatch(int id, [FromBody] UpdateMatchDto dto)
    {
        var match = await _context.Matches.FindAsync(id);
        if (match == null) return NotFound();

        // re chequear el overlap
        if (dto.StartTime.HasValue && dto.StartTime != match.StartTime)
        {
            var newStart = dto.StartTime.Value;
            var newEnd = dto.EndTime ?? newStart.AddHours(1);

            if (await HasDoubleBooking(match.Player1Id, newStart, newEnd, excludeMatchId: match.Id) ||
                await HasDoubleBooking(match.Player2Id, newStart, newEnd, excludeMatchId: match.Id))
            {
                return Conflict("Double-booking detected on update.");
            }

            match.StartTime = newStart;
        }

        if (dto.EndTime.HasValue) match.EndTime = dto.EndTime;
        if (dto.WinnerId.HasValue) match.WinnerId = dto.WinnerId;
        if (dto.TableNumber.HasValue) match.TableNumber = dto.TableNumber;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMatch(int id)
    {
        var match = await _context.Matches.FindAsync(id);
        if (match == null) return NotFound();

        if (match.StartTime <= DateTime.UtcNow)
        {
            return Conflict("Cannot delete a match that has already started.");
        }

        _context.Matches.Remove(match);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static MatchDto MapToDto(Match m)
    {
        string status = m.EndTime != null
            ? "completed"
            : (m.StartTime > DateTime.UtcNow ? "upcoming" : "ongoing");

        return new MatchDto
        {
            Id = m.Id,
            StartTime = m.StartTime,
            EndTime = m.EndTime,
            TableNumber = m.TableNumber,
            Player1Name = m.Player1!.Name,
            Player2Name = m.Player2!.Name,
            WinnerName = m.Winner?.Name,
            Status = status
        };
    }

    private async Task<bool> HasDoubleBooking(int playerId, DateTime start, DateTime end, int? excludeMatchId = null)
    {
        return await _context.Matches.AnyAsync(m =>
            (m.Player1Id == playerId || m.Player2Id == playerId) &&
            (!excludeMatchId.HasValue || m.Id != excludeMatchId.Value) &&
            (
                (m.EndTime ?? m.StartTime.AddHours(1)) > start &&
                m.StartTime < end
            )
        );
    }
}
