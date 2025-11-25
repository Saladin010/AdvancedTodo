using AdvancedTodoLearningCards.Models;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace AdvancedTodoLearningCards.Tests.Models
{
    public class CardModelTests
    {
        [Fact]
        public void Card_ShouldHaveRequiredProperties()
        {
            // Arrange & Act
            var card = new Card
            {
                UserId = "test-user",
                Title = "Test Title",
                Content = "Test Content",
                Difficulty = CardDifficulty.Medium
            };

            // Assert
            card.UserId.Should().Be("test-user");
            card.Title.Should().Be("Test Title");
            card.Content.Should().Be("Test Content");
            card.Difficulty.Should().Be(CardDifficulty.Medium);
        }

        [Fact]
        public void Card_ImageUrl_ShouldBeNullable()
        {
            // Arrange & Act
            var card = new Card
            {
                UserId = "test-user",
                Title = "Test",
                Content = "Content",
                Difficulty = CardDifficulty.Easy,
                ImageUrl = null
            };

            // Assert
            card.ImageUrl.Should().BeNull();
        }

        [Fact]
        public void Card_ImageUrl_ShouldAcceptValidUrl()
        {
            // Arrange & Act
            var card = new Card
            {
                UserId = "test-user",
                Title = "Test",
                Content = "Content",
                Difficulty = CardDifficulty.Easy,
                ImageUrl = "https://example.com/image.jpg"
            };

            // Assert
            card.ImageUrl.Should().Be("https://example.com/image.jpg");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Card_Title_ShouldFailValidation_WhenEmpty(string title)
        {
            // Arrange
            var card = new Card
            {
                UserId = "test-user",
                Title = title,
                Content = "Content",
                Difficulty = CardDifficulty.Easy
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(card);
            var isValid = Validator.TryValidateObject(card, context, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Title"));
        }

        [Fact]
        public void Card_Title_ShouldFailValidation_WhenTooLong()
        {
            // Arrange
            var card = new Card
            {
                UserId = "test-user",
                Title = new string('A', 201), // MaxLength is 200
                Content = "Content",
                Difficulty = CardDifficulty.Easy
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(card);
            var isValid = Validator.TryValidateObject(card, context, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Title"));
        }

        [Fact]
        public void Card_ImageUrl_ShouldFailValidation_WhenTooLong()
        {
            // Arrange
            var card = new Card
            {
                UserId = "test-user",
                Title = "Test",
                Content = "Content",
                Difficulty = CardDifficulty.Easy,
                ImageUrl = "https://example.com/" + new string('a', 500) // MaxLength is 500
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(card);
            var isValid = Validator.TryValidateObject(card, context, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("ImageUrl"));
        }

        [Fact]
        public void Card_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var card = new Card();

            // Assert
            card.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(CardDifficulty.Easy)]
        [InlineData(CardDifficulty.Medium)]
        [InlineData(CardDifficulty.Hard)]
        public void Card_ShouldAcceptAllDifficultyLevels(CardDifficulty difficulty)
        {
            // Arrange & Act
            var card = new Card
            {
                UserId = "test-user",
                Title = "Test",
                Content = "Content",
                Difficulty = difficulty
            };

            // Assert
            card.Difficulty.Should().Be(difficulty);
        }

        [Fact]
        public void Card_ShouldSetTimestamps()
        {
            // Arrange & Act
            var card = new Card
            {
                UserId = "test-user",
                Title = "Test",
                Content = "Content",
                Difficulty = CardDifficulty.Easy
            };

            // Assert
            card.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            card.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}
