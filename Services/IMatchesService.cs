using _8_ball_pool.DTOs.Match;
using _8_ball_pool.Models;
using Microsoft.AspNetCore.Mvc;

namespace _8_ball_pool.Services
{
    public interface IMatchesService
    {
        Task<Match> CreateMatch(CreateMatchDto dto);
        Task<IEnumerable<MatchDto>> GetMatches(DateTime? date, string? status);
        Task<MatchDto?> GetMatchById(int id);
        Task<bool> UpdateMatch(int id, UpdateMatchDto dto);
        Task<bool> DeleteMatch(int id);
        MatchDto MapToDto(Match match);
        Task<bool> HasDoubleBooking(int playerId, DateTime start, DateTime end, int? excludeMatchId = null);
    }
} 