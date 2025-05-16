using Microsoft.AspNetCore.Mvc;
using _8_ball_pool.DTOs.Match;
using _8_ball_pool.Services;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly IMatchesService _matchesService;

    public MatchesController(IMatchesService matchesService)
    {
        _matchesService = matchesService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateMatch([FromBody] CreateMatchDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var match = await _matchesService.CreateMatch(dto);
            return CreatedAtAction(nameof(GetMatchById), new { id = match.Id }, match);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatches([FromQuery] DateTime? date, [FromQuery] string? status)
    {
        var matches = await _matchesService.GetMatches(date, status);
        return Ok(matches);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MatchDto>> GetMatchById(int id)
    {
        var match = await _matchesService.GetMatchById(id);
        if (match == null) return NotFound();
        return match;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMatch(int id, [FromBody] UpdateMatchDto dto)
    {
        try
        {
            var result = await _matchesService.UpdateMatch(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMatch(int id)
    {
        try
        {
            var result = await _matchesService.DeleteMatch(id);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
