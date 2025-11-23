using AdvancedTodoLearningCards.Models;

namespace AdvancedTodoLearningCards.ViewModels
{
    public class ReviewSessionViewModel
    {
        public List<CardReviewItem> Cards { get; set; } = new List<CardReviewItem>();
        public int CurrentIndex { get; set; }
        public int TotalCards { get; set; }
        public bool ShowAnswer { get; set; }
        public DateTime SessionStartTime { get; set; } = DateTime.UtcNow;

        public CardReviewItem? CurrentCard =>
            CurrentIndex < Cards.Count ? Cards[CurrentIndex] : null;

        public int Progress => TotalCards > 0 ? (int)((CurrentIndex / (double)TotalCards) * 100) : 0;
    }

    public class CardReviewItem
    {
        public int CardId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public CardDifficulty Difficulty { get; set; }
        public int? LastInterval { get; set; }
        public DateTime? LastReviewedAt { get; set; }
    }

    public class ReviewResultViewModel
    {
        public int Quality { get; set; }
        public int CardId { get; set; }
        public int OldInterval { get; set; }
        public int NewInterval { get; set; }
        public decimal OldEaseFactor { get; set; }
        public decimal NewEaseFactor { get; set; }
        public DateTime NextReviewDate { get; set; }
    }

    public class SessionSummaryViewModel
    {
        public int TotalCardsReviewed { get; set; }
        public int CorrectRecalls { get; set; }
        public int FailedRecalls { get; set; }
        public double AverageQuality { get; set; }
        public TimeSpan SessionDuration { get; set; }
        public List<ReviewResultViewModel> Results { get; set; } = new List<ReviewResultViewModel>();
    }

    public class UpcomingReviewsViewModel
    {
        public List<UpcomingReviewItem> Reviews { get; set; } = new List<UpcomingReviewItem>();
    }

    public class UpcomingReviewItem
    {
        public int CardId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime NextReviewAt { get; set; }
        public int DaysUntilReview { get; set; }
        public CardDifficulty Difficulty { get; set; }
    }
}