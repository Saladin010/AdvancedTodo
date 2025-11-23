namespace AdvancedTodoLearningCards.ViewModels
{
    public class DashboardViewModel
    {
        // Statistics cards
        public int TotalCards { get; set; }
        public int DueToday { get; set; }
        public int ReviewedToday { get; set; }
        public int TotalReviews { get; set; }
        public double AverageQuality { get; set; }
        public double RetentionRate { get; set; }
        public int CurrentStreak { get; set; }

        // Chart data
        public ChartDataViewModel ReviewsOverTime { get; set; } = new ChartDataViewModel();
        public Dictionary<string, int> TagPerformance { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> DifficultyDistribution { get; set; } = new Dictionary<string, int>();

        // Upcoming reviews
        public List<UpcomingReviewPreview> UpcomingReviews { get; set; } = new List<UpcomingReviewPreview>();
    }

    public class ChartDataViewModel
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Data { get; set; } = new List<int>();
    }

    public class UpcomingReviewPreview
    {
        public int CardId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime NextReviewAt { get; set; }
        public string RelativeTime { get; set; } = string.Empty;
    }

    public class AdminSettingsViewModel
    {
        public string InitialIntervals { get; set; } = "[1,3,7,15,30]";
        public decimal InitialEaseFactor { get; set; } = 2.5m;
        public decimal MinimumEaseFactor { get; set; } = 1.3m;
        public decimal MaximumEaseFactor { get; set; } = 3.5m;
        public string DefaultSchedulingMode { get; set; } = "Fixed";
        public int SwitchToAdaptiveAfterReps { get; set; } = 5;
    }
}