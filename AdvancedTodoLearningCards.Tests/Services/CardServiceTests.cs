using AdvancedTodoLearningCards.Models;
using AdvancedTodoLearningCards.Repositories;
using AdvancedTodoLearningCards.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdvancedTodoLearningCards.Tests.Services
{
    public class CardServiceTests
    {
        private readonly Mock<ICardRepository> _mockCardRepository;
        private readonly Mock<IRepository<CardSchedule>> _mockScheduleRepository;
        private readonly Mock<ISchedulingEngine> _mockSchedulingEngine;
        private readonly Mock<ILogger<CardService>> _mockLogger;
        private readonly CardService _cardService;
        private readonly string _testUserId = "test-user-123";

        public CardServiceTests()
        {
            _mockCardRepository = new Mock<ICardRepository>();
            _mockScheduleRepository = new Mock<IRepository<CardSchedule>>();
            _mockSchedulingEngine = new Mock<ISchedulingEngine>();
            _mockLogger = new Mock<ILogger<CardService>>();

            _cardService = new CardService(
                _mockCardRepository.Object,
                _mockScheduleRepository.Object,
                _mockSchedulingEngine.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreateCardAsync_ShouldCreateCard_AndInitializeSchedule()
        {
            // Arrange
            var card = new Card
            {
                UserId = _testUserId,
                Title = "Test Card",
                Content = "Test Content",
                Difficulty = CardDifficulty.Medium,
                ImageUrl = "https://example.com/image.jpg"
            };

            var schedule = new CardSchedule { CardId = 1 };

            _mockCardRepository.Setup(r => r.AddAsync(It.IsAny<Card>())).Returns(Task.CompletedTask);
            _mockCardRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mockSchedulingEngine.Setup(e => e.InitializeSchedule(It.IsAny<int>())).Returns(schedule);
            _mockScheduleRepository.Setup(r => r.AddAsync(It.IsAny<CardSchedule>())).Returns(Task.CompletedTask);
            _mockScheduleRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _cardService.CreateCardAsync(card);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Test Card");
            result.ImageUrl.Should().Be("https://example.com/image.jpg");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            
            _mockCardRepository.Verify(r => r.AddAsync(It.IsAny<Card>()), Times.Once);
            _mockSchedulingEngine.Verify(e => e.InitializeSchedule(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetAllCardsAsync_ShouldReturnUserCards()
        {
            // Arrange
            var cards = new List<Card>
            {
                new Card { Id = 1, UserId = _testUserId, Title = "Card 1", Content = "Content 1", Difficulty = CardDifficulty.Easy },
                new Card { Id = 2, UserId = _testUserId, Title = "Card 2", Content = "Content 2", Difficulty = CardDifficulty.Medium }
            };

            _mockCardRepository.Setup(r => r.GetCardsByUserIdWithScheduleAsync(_testUserId))
                .ReturnsAsync(cards);

            // Act
            var result = await _cardService.GetAllCardsAsync(_testUserId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(c => c.UserId.Should().Be(_testUserId));
        }

        [Fact]
        public async Task GetCardByIdAsync_ShouldReturnCard_WhenExists()
        {
            // Arrange
            var card = new Card
            {
                Id = 1,
                UserId = _testUserId,
                Title = "Test Card",
                Content = "Test Content",
                Difficulty = CardDifficulty.Easy
            };

            _mockCardRepository.Setup(r => r.GetCardWithScheduleAsync(1))
                .ReturnsAsync(card);

            // Act
            var result = await _cardService.GetCardByIdAsync(1, _testUserId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Title.Should().Be("Test Card");
        }

        [Fact]
        public async Task GetCardByIdAsync_ShouldReturnNull_WhenCardBelongsToOtherUser()
        {
            // Arrange
            var card = new Card
            {
                Id = 1,
                UserId = "other-user",
                Title = "Test Card",
                Content = "Test Content",
                Difficulty = CardDifficulty.Easy
            };

            _mockCardRepository.Setup(r => r.GetCardWithScheduleAsync(1))
                .ReturnsAsync(card);

            // Act
            var result = await _cardService.GetCardByIdAsync(1, _testUserId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateCardAsync_ShouldUpdateCard()
        {
            // Arrange
            var card = new Card
            {
                Id = 1,
                UserId = _testUserId,
                Title = "Updated Title",
                Content = "Updated Content",
                Difficulty = CardDifficulty.Hard,
                ImageUrl = "https://example.com/new-image.jpg"
            };

            _mockCardRepository.Setup(r => r.Update(It.IsAny<Card>()));
            _mockCardRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _cardService.UpdateCardAsync(card);

            // Assert
            result.Should().NotBeNull();
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            _mockCardRepository.Verify(r => r.Update(It.IsAny<Card>()), Times.Once);
            _mockCardRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCardAsync_ShouldDeleteCard_WhenExists()
        {
            // Arrange
            var card = new Card
            {
                Id = 1,
                UserId = _testUserId,
                Title = "Test Card",
                Content = "Test Content",
                Difficulty = CardDifficulty.Medium
            };

            _mockCardRepository.Setup(r => r.GetCardWithScheduleAsync(1))
                .ReturnsAsync(card);
            _mockCardRepository.Setup(r => r.Remove(It.IsAny<Card>()));
            _mockCardRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _cardService.DeleteCardAsync(1, _testUserId);

            // Assert
            result.Should().BeTrue();
            _mockCardRepository.Verify(r => r.Remove(It.IsAny<Card>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCardAsync_ShouldReturnFalse_WhenCardBelongsToOtherUser()
        {
            // Arrange
            var card = new Card
            {
                Id = 1,
                UserId = "other-user",
                Title = "Test Card",
                Content = "Test Content",
                Difficulty = CardDifficulty.Medium
            };

            _mockCardRepository.Setup(r => r.GetCardWithScheduleAsync(1))
                .ReturnsAsync(card);

            // Act
            var result = await _cardService.DeleteCardAsync(1, _testUserId);

            // Assert
            result.Should().BeFalse();
            _mockCardRepository.Verify(r => r.Remove(It.IsAny<Card>()), Times.Never);
        }

        [Fact]
        public async Task SearchCardsAsync_ShouldReturnMatchingCards()
        {
            // Arrange
            var cards = new List<Card>
            {
                new Card { Id = 1, UserId = _testUserId, Title = "JavaScript Basics", Content = "Learn JS", Difficulty = CardDifficulty.Easy }
            };

            _mockCardRepository.Setup(r => r.SearchCardsAsync(_testUserId, "JavaScript"))
                .ReturnsAsync(cards);

            // Act
            var result = await _cardService.SearchCardsAsync(_testUserId, "JavaScript");

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Contain("JavaScript");
        }
    }
}
