using AdvancedTodoLearningCards.Models;
using AdvancedTodoLearningCards.Services;
using FluentAssertions;
using Xunit;

namespace AdvancedTodoLearningCards.Tests.Services
{
    public class Sm2SchedulingEngineTests
    {
        private readonly Sm2SchedulingEngine _engine;

        public Sm2SchedulingEngineTests()
        {
            _engine = new Sm2SchedulingEngine();
        }

        [Fact]
        public void InitializeSchedule_ShouldCreateSchedule_WithDefaultValues()
        {
            // Arrange
            int cardId = 1;

            // Act
            var schedule = _engine.InitializeSchedule(cardId);

            // Assert
            schedule.Should().NotBeNull();
            schedule.CardId.Should().Be(cardId);
            schedule.IntervalDays.Should().BeGreaterThan(0);
            schedule.EaseFactor.Should().Be(2.5m);
            schedule.RepetitionNumber.Should().Be(0);
        }

        [Fact]
        public void CalculateNextReview_ShouldUpdateSchedule()
        {
            // Arrange
            var schedule = new CardSchedule
            {
                CardId = 1,
                IntervalDays = 1,
                EaseFactor = 2.5m,
                RepetitionNumber = 0
            };

            // Act
            var result = _engine.CalculateNextReview(schedule, 4);

            // Assert
            result.Should().NotBeNull();
            result.NextReviewAt.Should().BeAfter(DateTime.UtcNow);
            result.LastReviewedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.ReviewCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void CalculateNextReview_QualityZero_ShouldIncreaseLapseCount()
        {
            // Arrange
            var schedule = new CardSchedule
            {
                CardId = 1,
                IntervalDays = 30,
                EaseFactor = 2.8m,
                RepetitionNumber = 5,
                LapseCount = 0
            };

            // Act
            var result = _engine.CalculateNextReview(schedule, 0);

            // Assert
            result.LapseCount.Should().BeGreaterThan(0);
            result.EaseFactor.Should().BeLessThanOrEqualTo(2.8m);
        }

        [Fact]
        public void CalculateNextReview_HighQuality_ShouldMaintainOrIncreaseEaseFactor()
        {
            // Arrange
            var initialEaseFactor = 2.5m;
            var schedule = new CardSchedule
            {
                CardId = 1,
                IntervalDays = 6,
                EaseFactor = initialEaseFactor,
                RepetitionNumber = 1
            };

            // Act
            var result = _engine.CalculateNextReview(schedule, 5); // Perfect quality

            // Assert
            result.EaseFactor.Should().BeGreaterThanOrEqualTo(initialEaseFactor);
            result.NextReviewAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public void CalculateNextReview_LowQuality_ShouldDecreaseEaseFactor()
        {
            // Arrange
            var initialEaseFactor = 2.5m;
            var schedule = new CardSchedule
            {
                CardId = 1,
                IntervalDays = 6,
                EaseFactor = initialEaseFactor,
                RepetitionNumber = 1
            };

            // Act
            var result = _engine.CalculateNextReview(schedule, 1); // Hard quality

            // Assert
            result.EaseFactor.Should().BeLessThan(initialEaseFactor);
        }

        [Fact]
        public void CalculateNextReview_EaseFactor_ShouldNotGoBelowMinimum()
        {
            // Arrange
            var schedule = new CardSchedule
            {
                CardId = 1,
                IntervalDays = 6,
                EaseFactor = 1.3m, // Already at minimum
                RepetitionNumber = 1
            };

            // Act
            var result = _engine.CalculateNextReview(schedule, 0); // Worst quality

            // Assert
            result.EaseFactor.Should().BeGreaterThanOrEqualTo(1.3m);
        }

        [Fact]
        public void CalculateNextReview_ShouldIncrementReviewCount()
        {
            // Arrange
            var schedule = new CardSchedule
            {
                CardId = 1,
                IntervalDays = 1,
                EaseFactor = 2.5m,
                RepetitionNumber = 0,
                ReviewCount = 5
            };

            // Act
            var result = _engine.CalculateNextReview(schedule, 4);

            // Assert
            result.ReviewCount.Should().Be(6);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void CalculateNextReview_ShouldHandleAllQualityLevels(int quality)
        {
            // Arrange
            var schedule = new CardSchedule
            {
                CardId = 1,
                IntervalDays = 1,
                EaseFactor = 2.5m,
                RepetitionNumber = 0
            };

            // Act
            var result = _engine.CalculateNextReview(schedule, quality);

            // Assert
            result.Should().NotBeNull();
            result.NextReviewAt.Should().BeAfter(DateTime.UtcNow);
            result.EaseFactor.Should().BeInRange(1.3m, 3.5m);
        }
    }
}
