using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _8_ball_pool.Controllers;
using _8_ball_pool.DTOs.Match;
using _8_ball_pool.Models;
using _8_ball_pool.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Match = _8_ball_pool.Models.Match;

namespace EightBallPool.Tests.Controllers
{
    public class MatchesControllerTests
    {
        private readonly Mock<IMatchesService> _mockMatchesService;
        private readonly MatchesController _controller;

        public MatchesControllerTests()
        {
            _mockMatchesService = new Mock<IMatchesService>();
            _controller = new MatchesController(_mockMatchesService.Object);
        }

        [Fact]
        public async Task CreateMatch_ValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateMatchDto
            {
                Player1Id = 1,
                Player2Id = 2,
                StartTime = DateTime.UtcNow.AddDays(1),
                TableNumber = 1
            };

            var match = new Match
            {
                Id = 1,
                Player1Id = dto.Player1Id,
                Player2Id = dto.Player2Id,
                StartTime = dto.StartTime,
                TableNumber = dto.TableNumber
            };

            _mockMatchesService.Setup(s => s.CreateMatch(It.IsAny<CreateMatchDto>()))
                .ReturnsAsync(match);

            // Act
            var result = await _controller.CreateMatch(dto);

            // Assert
            var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(MatchesController.GetMatchById));
            createdAtActionResult.RouteValues?["id"].Should().Be(1);
            createdAtActionResult.Value.Should().Be(match);
        }

        [Fact]
        public async Task CreateMatch_DoubleBooking_ReturnsConflict()
        {
            // Arrange
            var dto = new CreateMatchDto
            {
                Player1Id = 1,
                Player2Id = 2,
                StartTime = DateTime.UtcNow.AddDays(1),
                TableNumber = 1
            };

            _mockMatchesService.Setup(s => s.CreateMatch(It.IsAny<CreateMatchDto>()))
                .ThrowsAsync(new InvalidOperationException("Double booking"));

            // Act
            var result = await _controller.CreateMatch(dto);

            // Assert
            var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.Value.Should().Be("Double booking");
        }

        [Fact]
        public async Task GetMatches_ReturnsOkWithMatches()
        {
            // Arrange
            var matches = new List<MatchDto>
            {
                new MatchDto
                {
                    Id = 1,
                    Player1Name = "Player 1",
                    Player2Name = "Player 2",
                    StartTime = DateTime.UtcNow,
                    Status = "ongoing"
                },
                new MatchDto
                {
                    Id = 2,
                    Player1Name = "Player 2",
                    Player2Name = "Player 3",
                    StartTime = DateTime.UtcNow.AddDays(1),
                    Status = "upcoming"
                }
            };

            _mockMatchesService.Setup(s => s.GetMatches(It.IsAny<DateTime?>(), It.IsAny<string>()))
                .ReturnsAsync(matches);

            // Act
            var result = await _controller.GetMatches(null, null);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedMatches = okResult.Value.Should().BeAssignableTo<IEnumerable<MatchDto>>().Subject;
            returnedMatches.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetMatchById_ExistingId_ReturnsMatch()
        {
            // Arrange
            var matchDto = new MatchDto
            {
                Id = 1,
                Player1Name = "Player 1",
                Player2Name = "Player 2",
                StartTime = DateTime.UtcNow,
                Status = "ongoing"
            };

            _mockMatchesService.Setup(s => s.GetMatchById(1))
                .ReturnsAsync(matchDto);

            // Act
            var result = await _controller.GetMatchById(1);

            // Assert
            result.Value.Should().BeEquivalentTo(matchDto);
        }

        [Fact]
        public async Task GetMatchById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockMatchesService.Setup(s => s.GetMatchById(999))
                .ReturnsAsync((MatchDto?)null);

            // Act
            var result = await _controller.GetMatchById(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateMatch_ValidData_ReturnsNoContent()
        {
            // Arrange
            var dto = new UpdateMatchDto
            {
                TableNumber = 2,
                WinnerId = 1
            };

            _mockMatchesService.Setup(s => s.UpdateMatch(1, It.IsAny<UpdateMatchDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateMatch(1, dto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task UpdateMatch_NonExistingMatch_ReturnsNotFound()
        {
            // Arrange
            var dto = new UpdateMatchDto
            {
                TableNumber = 2
            };

            _mockMatchesService.Setup(s => s.UpdateMatch(999, It.IsAny<UpdateMatchDto>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateMatch(999, dto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateMatch_DoubleBooking_ReturnsConflict()
        {
            // Arrange
            var dto = new UpdateMatchDto
            {
                StartTime = DateTime.UtcNow.AddDays(1)
            };

            _mockMatchesService.Setup(s => s.UpdateMatch(1, It.IsAny<UpdateMatchDto>()))
                .ThrowsAsync(new InvalidOperationException("Double booking"));

            // Act
            var result = await _controller.UpdateMatch(1, dto);

            // Assert
            var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.Value.Should().Be("Double booking");
        }

        [Fact]
        public async Task DeleteMatch_ExistingMatch_ReturnsNoContent()
        {
            // Arrange
            _mockMatchesService.Setup(s => s.DeleteMatch(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteMatch(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteMatch_NonExistingMatch_ReturnsNotFound()
        {
            // Arrange
            _mockMatchesService.Setup(s => s.DeleteMatch(999))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteMatch(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteMatch_StartedMatch_ReturnsConflict()
        {
            // Arrange
            _mockMatchesService.Setup(s => s.DeleteMatch(1))
                .ThrowsAsync(new InvalidOperationException("Cannot delete started match"));

            // Act
            var result = await _controller.DeleteMatch(1);

            // Assert
            var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.Value.Should().Be("Cannot delete started match");
        }
    }
} 