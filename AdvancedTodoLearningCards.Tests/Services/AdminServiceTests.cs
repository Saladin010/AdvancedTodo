using AdvancedTodoLearningCards.Data;
using AdvancedTodoLearningCards.Models;
using AdvancedTodoLearningCards.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AdvancedTodoLearningCards.Tests.Services
{
    public class AdminServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AdminService _adminService;

        public AdminServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _adminService = new AdminService(_context);
        }

        [Fact]
        public async Task GetDashboardStatisticsAsync_ShouldReturnCorrectCounts()
        {
            // Arrange
            var user1 = new ApplicationUser { Id = "user1", UserName = "user1@test.com", Email = "user1@test.com", CreatedAt = DateTime.UtcNow };
            var user2 = new ApplicationUser { Id = "user2", UserName = "user2@test.com", Email = "user2@test.com", CreatedAt = DateTime.UtcNow };
            
            await _context.Users.AddRangeAsync(user1, user2);
            
            await _context.Cards.AddRangeAsync(
                new Card { UserId = "user1", Title = "Card 1", Content = "Content", Difficulty = CardDifficulty.Easy, CreatedAt = DateTime.UtcNow },
                new Card { UserId = "user1", Title = "Card 2", Content = "Content", Difficulty = CardDifficulty.Medium, CreatedAt = DateTime.UtcNow },
                new Card { UserId = "user2", Title = "Card 3", Content = "Content", Difficulty = CardDifficulty.Hard, CreatedAt = DateTime.UtcNow }
            );
            
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminService.GetDashboardStatisticsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalUsers.Should().Be(2);
            result.TotalCards.Should().Be(3);
            result.AverageCardsPerUser.Should().Be(1.5);
        }

        [Fact]
        public async Task GetCardsByDifficultyAsync_ShouldReturnCorrectDistribution()
        {
            // Arrange
            await _context.Cards.AddRangeAsync(
                new Card { UserId = "user1", Title = "Easy 1", Content = "Content", Difficulty = CardDifficulty.Easy, CreatedAt = DateTime.UtcNow },
                new Card { UserId = "user1", Title = "Easy 2", Content = "Content", Difficulty = CardDifficulty.Easy, CreatedAt = DateTime.UtcNow },
                new Card { UserId = "user1", Title = "Medium 1", Content = "Content", Difficulty = CardDifficulty.Medium, CreatedAt = DateTime.UtcNow },
                new Card { UserId = "user1", Title = "Hard 1", Content = "Content", Difficulty = CardDifficulty.Hard, CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminService.GetCardsByDifficultyAsync();

            // Assert
            result.Should().ContainKey("Easy").WhoseValue.Should().Be(2);
            result.Should().ContainKey("Medium").WhoseValue.Should().Be(1);
            result.Should().ContainKey("Hard").WhoseValue.Should().Be(1);
        }

        [Fact]
        public async Task GetDailyReviewsAsync_ShouldReturnDataForLast30Days()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "user1@test.com", Email = "user1@test.com", CreatedAt = DateTime.UtcNow };
            var card = new Card { UserId = "user1", Title = "Card", Content = "Content", Difficulty = CardDifficulty.Easy, CreatedAt = DateTime.UtcNow };
            
            await _context.Users.AddAsync(user);
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();

            await _context.ReviewLogs.AddRangeAsync(
                new ReviewLog { UserId = "user1", CardId = card.Id, Quality = 4, ReviewedAt = DateTime.UtcNow.AddDays(-5) },
                new ReviewLog { UserId = "user1", CardId = card.Id, Quality = 5, ReviewedAt = DateTime.UtcNow.AddDays(-5) },
                new ReviewLog { UserId = "user1", CardId = card.Id, Quality = 3, ReviewedAt = DateTime.UtcNow.AddDays(-10) }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminService.GetDailyReviewsAsync(30);

            // Assert
            result.Should().HaveCount(30);
            result.Should().Contain(d => d.Date.Date == DateTime.UtcNow.AddDays(-5).Date && d.Count == 2);
            result.Should().Contain(d => d.Date.Date == DateTime.UtcNow.AddDays(-10).Date && d.Count == 1);
        }

        [Fact]
        public async Task GetUserStatisticsAsync_ShouldReturnAllUsers_WhenNoSearchTerm()
        {
            // Arrange
            await _context.Users.AddRangeAsync(
                new ApplicationUser { Id = "user1", UserName = "alice@test.com", Email = "alice@test.com", CreatedAt = DateTime.UtcNow },
                new ApplicationUser { Id = "user2", UserName = "bob@test.com", Email = "bob@test.com", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminService.GetUserStatisticsAsync();

            // Assert
            result.Users.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUserStatisticsAsync_ShouldFilterBySearchTerm()
        {
            // Arrange
            await _context.Users.AddRangeAsync(
                new ApplicationUser { Id = "user1", UserName = "alice@test.com", Email = "alice@test.com", CreatedAt = DateTime.UtcNow },
                new ApplicationUser { Id = "user2", UserName = "bob@test.com", Email = "bob@test.com", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminService.GetUserStatisticsAsync("alice");

            // Assert
            result.Users.Should().HaveCount(1);
            result.Users.First().UserName.Should().Contain("alice");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
