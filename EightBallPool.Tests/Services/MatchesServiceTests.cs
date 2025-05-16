using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _8_ball_pool.Data;
using _8_ball_pool.DTOs.Match;
using _8_ball_pool.Models;
using _8_ball_pool.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Match = _8_ball_pool.Models.Match;

namespace EightBallPool.Tests.Services
{
    public class MatchesServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly MatchesService _service;
        private readonly Player _player1;
        private readonly Player _player2;
        private readonly Player _player3;

        public MatchesServiceTests()
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
            _player3 = new Player { Id = 3, Name = "Player 3", ProfilePictureUrl = "url3" };
            
            _context.Players.AddRange(_player1, _player2, _player3);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateMatch_ValidData_ReturnsNewMatch()
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
                Player2Id = _player3.Id,
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
                Player2Id = _player3.Id,
                StartTime = DateTime.UtcNow.AddHours(1),
                Player1 = _player2,
                Player2 = _player3
            };

            _context.Matches.AddRange(match1, match2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMatches(null, null);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetMatches_WithDateFilter_ReturnsFilteredMatches()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            
            var match1 = new Match
            {
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = today.AddHours(10),
                Player1 = _player1,
                Player2 = _player2
            };

            var match2 = new Match
            {
                Player1Id = _player2.Id,
                Player2Id = _player3.Id,
                StartTime = tomorrow.AddHours(10),
                Player1 = _player2,
                Player2 = _player3
            };

            _context.Matches.AddRange(match1, match2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMatches(today, null);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(match1.Id);
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
        public async Task GetMatchById_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _service.GetMatchById(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateMatch_ValidData_ReturnsTrue()
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
        public async Task UpdateMatch_NonExistingMatch_ReturnsFalse()
        {
            // Arrange
            var dto = new UpdateMatchDto
            {
                TableNumber = 2
            };

            // Act
            var result = await _service.UpdateMatch(999, dto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteMatch_ExistingFutureMatch_ReturnsTrue()
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

        [Fact]
        public async Task DeleteMatch_NonExistingMatch_ReturnsFalse()
        {
            // Act
            var result = await _service.DeleteMatch(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void MapToDto_ReturnsCorrectDto()
        {
            // Arrange
            var match = new Match
            {
                Id = 1,
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow,
                WinnerId = _player1.Id,
                TableNumber = 3,
                Player1 = _player1,
                Player2 = _player2,
                Winner = _player1
            };

            // Act
            var result = _service.MapToDto(match);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(match.Id);
            result.StartTime.Should().Be(match.StartTime);
            result.EndTime.Should().Be(match.EndTime);
            result.TableNumber.Should().Be(match.TableNumber);
            result.Player1Name.Should().Be(_player1.Name);
            result.Player2Name.Should().Be(_player2.Name);
            result.WinnerName.Should().Be(_player1.Name);
            result.Status.Should().Be("completed");
        }

        [Fact]
        public async Task HasDoubleBooking_DetectsOverlap()
        {
            // Arrange
            var match = new Match
            {
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = DateTime.UtcNow.AddHours(1),
                Player1 = _player1,
                Player2 = _player2
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.HasDoubleBooking(_player1.Id, DateTime.UtcNow.AddHours(1.5), DateTime.UtcNow.AddHours(2.5));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasDoubleBooking_NoOverlap_ReturnsFalse()
        {
            // Arrange
            var match = new Match
            {
                Player1Id = _player1.Id,
                Player2Id = _player2.Id,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Player1 = _player1,
                Player2 = _player2
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.HasDoubleBooking(_player1.Id, DateTime.UtcNow.AddHours(3), DateTime.UtcNow.AddHours(4));

            // Assert
            result.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 