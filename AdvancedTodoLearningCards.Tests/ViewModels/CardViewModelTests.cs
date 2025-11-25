using AdvancedTodoLearningCards.Models;
using AdvancedTodoLearningCards.ViewModels;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace AdvancedTodoLearningCards.Tests.ViewModels
{
    public class CardViewModelTests
    {
        [Fact]
        public void CreateCardViewModel_ShouldValidate_RequiredFields()
        {
            // Arrange
            var viewModel = new CreateCardViewModel
            {
                Title = "",
                Content = "",
                Difficulty = CardDifficulty.Easy
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(viewModel);
            var isValid = Validator.TryValidateObject(viewModel, context, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Title"));
            validationResults.Should().Contain(v => v.MemberNames.Contains("Content"));
        }

        [Fact]
        public void CreateCardViewModel_ShouldAccept_ValidImageUrl()
        {
            // Arrange
            var viewModel = new CreateCardViewModel
            {
                Title = "Test Card",
                Content = "Test Content",
                Difficulty = CardDifficulty.Medium,
                ImageUrl = "https://example.com/image.jpg"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(viewModel);
            var isValid = Validator.TryValidateObject(viewModel, context, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            viewModel.ImageUrl.Should().Be("https://example.com/image.jpg");
        }

        [Fact]
        public void CreateCardViewModel_ShouldReject_InvalidImageUrl()
        {
            // Arrange
            var viewModel = new CreateCardViewModel
            {
                Title = "Test Card",
                Content = "Test Content",
                Difficulty = CardDifficulty.Medium,
                ImageUrl = "not-a-valid-url"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(viewModel);
            var isValid = Validator.TryValidateObject(viewModel, context, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("ImageUrl"));
        }

        [Fact]
        public void CreateCardViewModel_ShouldAccept_NullImageUrl()
        {
            // Arrange
            var viewModel = new CreateCardViewModel
            {
                Title = "Test Card",
                Content = "Test Content",
                Difficulty = CardDifficulty.Easy,
                ImageUrl = null
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(viewModel);
            var isValid = Validator.TryValidateObject(viewModel, context, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void CardViewModel_ShouldMap_AllProperties()
        {
            // Arrange & Act
            var viewModel = new CardViewModel
            {
                Id = 1,
                Title = "Test Title",
                Content = "Test Content",
                Tags = new List<string> { "tag1", "tag2" },
                Difficulty = CardDifficulty.Hard,
                ImageUrl = "https://example.com/image.jpg",
                NextReviewAt = DateTime.UtcNow.AddDays(1),
                ReviewCount = 5
            };

            // Assert
            viewModel.Id.Should().Be(1);
            viewModel.Title.Should().Be("Test Title");
            viewModel.Content.Should().Be("Test Content");
            viewModel.Tags.Should().HaveCount(2);
            viewModel.Difficulty.Should().Be(CardDifficulty.Hard);
            viewModel.ImageUrl.Should().Be("https://example.com/image.jpg");
            viewModel.ReviewCount.Should().Be(5);
        }

        [Fact]
        public void EditCardViewModel_ShouldPreserve_CardId()
        {
            // Arrange & Act
            var viewModel = new EditCardViewModel
            {
                Id = 42,
                Title = "Updated Title",
                Content = "Updated Content",
                Difficulty = CardDifficulty.Medium,
                ImageUrl = "https://example.com/new-image.jpg"
            };

            // Assert
            viewModel.Id.Should().Be(42);
            viewModel.ImageUrl.Should().Be("https://example.com/new-image.jpg");
        }
    }
}
