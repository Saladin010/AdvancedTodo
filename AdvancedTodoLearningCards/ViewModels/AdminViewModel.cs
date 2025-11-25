using AdvancedTodoLearningCards.Models;

namespace AdvancedTodoLearningCards.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCards { get; set; }
        public int TotalReviews { get; set; }
        public int ActiveUsersToday { get; set; }
        public int ActiveUsersThisWeek { get; set; }
        public int CardsCreatedToday { get; set; }
        public int ReviewsCompletedToday { get; set; }
        public double AverageCardsPerUser { get; set; }
        public double AverageReviewsPerUser { get; set; }

        // Chart data
        public List<DailyStatistic> DailyReviews { get; set; } = new List<DailyStatistic>();
        public List<DailyStatistic> DailyNewCards { get; set; } = new List<DailyStatistic>();
        public Dictionary<string, int> CardsByDifficulty { get; set; } = new Dictionary<string, int>();
        
        // Recent activity
        public List<RecentActivityItem> RecentActivity { get; set; } = new List<RecentActivityItem>();
    }

    public class DailyStatistic
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class RecentActivityItem
    {
        public string UserName { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class UserManagementViewModel
    {
        public List<UserStatistics> Users { get; set; } = new List<UserStatistics>();
        public string? SearchTerm { get; set; }
    }

    public class UserStatistics
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int CardCount { get; set; }
        public int ReviewCount { get; set; }
        public int ReviewsToday { get; set; }
        public double AverageQuality { get; set; }
        public int DaysActive { get; set; }
    }
}
