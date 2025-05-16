using _8_ball_pool.Data;
using _8_ball_pool.DTOs.Match;
using _8_ball_pool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _8_ball_pool.Services
{
    public class MatchesService : IMatchesService
    {
        private readonly AppDbContext _context;

        public MatchesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Match> CreateMatch(CreateMatchDto dto)
        {
            var endTime = dto.StartTime.AddHours(1); // Default duration is 1 hour

            if (await HasDoubleBooking(dto.Player1Id, dto.StartTime, endTime) ||
                await HasDoubleBooking(dto.Player2Id, dto.StartTime, endTime))
            {
                throw new InvalidOperationException("One of the players has another match at this time.");
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

            return match;
        }

        public async Task<bool> DeleteMatch(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return false;

            if (match.StartTime <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Cannot delete a match that has already started.");
            }

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MatchDto?> GetMatchById(int id)
        {
            var match = await _context.Matches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.Winner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null) return null;

            return MapToDto(match);
        }

        public async Task<IEnumerable<MatchDto>> GetMatches(DateTime? date, string? status)
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
            return matches.Select(MapToDto);
        }

        public async Task<bool> UpdateMatch(int id, UpdateMatchDto dto)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return false;

            // Check for overlap if start time changes
            if (dto.StartTime.HasValue && dto.StartTime != match.StartTime)
            {
                var newStart = dto.StartTime.Value;
                var newEnd = dto.EndTime ?? newStart.AddHours(1);

                if (await HasDoubleBooking(match.Player1Id, newStart, newEnd, excludeMatchId: match.Id) ||
                    await HasDoubleBooking(match.Player2Id, newStart, newEnd, excludeMatchId: match.Id))
                {
                    throw new InvalidOperationException("Double-booking detected on update.");
                }

                match.StartTime = newStart;
            }

            if (dto.EndTime.HasValue) match.EndTime = dto.EndTime;
            if (dto.WinnerId.HasValue) match.WinnerId = dto.WinnerId;
            if (dto.TableNumber.HasValue) match.TableNumber = dto.TableNumber;

            await _context.SaveChangesAsync();
            return true;
        }

        public MatchDto MapToDto(Match m)
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

        public async Task<bool> HasDoubleBooking(int playerId, DateTime start, DateTime end, int? excludeMatchId = null)
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
} 