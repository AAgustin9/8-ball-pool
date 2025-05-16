using System;
using System.Linq;
using System.Threading.Tasks;
using _8_ball_pool.Data;
using _8_ball_pool.DTOs.Match;
using _8_ball_pool.Models;
using _8_ball_pool.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EightBallPool.Tests.Services
{
    public class MatchesServiceIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly MatchesService _service;
        private readonly Player _player1;
        private readonly Player _player2;

        public MatchesServiceIntegrationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _service = new MatchesService(_context);

            // Seed test data
            _player1 = new Player { Id = 1, Name = "Player 1", ProfilePictureUrl = "url1" };
            _player2 = new Player { Id = 2, Name = "Player 2", ProfilePictureUrl = "url2" };
            
            _context.Players.Add(_player1);
            _context.Players.Add(_player2);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateMatch_ValidData_CreatesMatch()
        {
            // Arrange
            var dto = new CreateMatchDto
            {
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = DateTime.UtcNow.AddDays(1),
                TableNumber = 1
            };

            // Act
            var result = await _service.CreateMatch(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Player1Id.Should().Be(dto.Player1Id);
            result.Player2Id.Should().Be(dto.Player2Id);
            result.StartTime.Should().Be(dto.StartTime);
            result.TableNumber.Should().Be(dto.TableNumber);

            // Verify in database
            var matchInDb = await _context.Matches.FindAsync(result.Id);
            matchInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateMatch_DoubleBooking_ThrowsException()
        {
            // Arrange
            var existingMatch = new Match
            {
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = DateTime.UtcNow.AddDays(1).AddHours(1),
                TableNumber = 1
            };
            _context.Matches.Add(existingMatch);
            await _context.SaveChangesAsync();

            var dto = new CreateMatchDto
            {
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = DateTime.UtcNow.AddDays(1), // Overlaps with existing match
                TableNumber = 2
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateMatch(dto));
        }

        [Fact]
        public async Task GetMatches_ReturnsAllMatches()
        {
            // Arrange
            var match1 = new Match
            {
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = DateTime.UtcNow.AddHours(-1),
                Player1 = _player1,
                Player2 = _player2
            };

            var match2 = new Match
            {
                Player1Id = _player2.Id,
                Player2Id = _player1.Id,
                StartTime = DateTime.UtcNow.AddHours(1),
                Player1 = _player2,
                Player2 = _player1
            };

            _context.Matches.AddRange(match1, match2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMatches(null, null);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetMatchById_ExistingId_ReturnsMatch()
        {
            // Arrange
            var match = new Match
            {
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = DateTime.UtcNow,
                Player1 = _player1,
                Player2 = _player2
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMatchById(match.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(match.Id);
            result.Player1Name.Should().Be(_player1.Name);
            result.Player2Name.Should().Be(_player2.Name);
        }

        [Fact]
        public async Task UpdateMatch_ValidData_UpdatesMatch()
        {
            // Arrange
            var match = new Match
            {
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = DateTime.UtcNow.AddDays(1),
                TableNumber = 1
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            var dto = new UpdateMatchDto
            {
                TableNumber = 2,
                WinnerId = _player1.Id
            };

            // Act
            var result = await _service.UpdateMatch(match.Id, dto);

            // Assert
            result.Should().BeTrue();

            // Verify in database
            var updatedMatch = await _context.Matches.FindAsync(match.Id);
            updatedMatch.Should().NotBeNull();
            updatedMatch!.TableNumber.Should().Be(dto.TableNumber);
            updatedMatch.WinnerId.Should().Be(dto.WinnerId);
        }

        [Fact]
        public async Task DeleteMatch_ExistingFutureMatch_DeletesMatch()
        {
            // Arrange
            var match = new Match
            {
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = DateTime.UtcNow.AddDays(1)
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteMatch(match.Id);

            // Assert
            result.Should().BeTrue();

            // Verify in database
            var deletedMatch = await _context.Matches.FindAsync(match.Id);
            deletedMatch.Should().BeNull();
        }

        [Fact]
        public async Task DeleteMatch_StartedMatch_ThrowsException()
        {
            // Arrange
            var match = new Match
            {
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = DateTime.UtcNow.AddHours(-1) // Match already started
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteMatch(match.Id));

            // Verify match still exists
            var matchInDb = await _context.Matches.FindAsync(match.Id);
            matchInDb.Should().NotBeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 