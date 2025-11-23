namespace AdvancedTodoLearningCards.Services
{
    /// </summary>
    public interface IDashboardService
    {
        Task<DashboardStats> GetDashboardStatsAsync(string userId);
        Task<Dictionary<DateTime, int>> GetReviewChartDataAsync(string userId, int days = 30);
        Task<Dictionary<string, int>> GetTagPerformanceAsync(string userId);
        Task<Dictionary<string, int>> GetDifficultyDistributionAsync(string userId);
        Task<double> GetRetentionRateAsync(string userId);
    }

    public class DashboardStats
    {
        public int TotalCards { get; set; }
        public int DueToday { get; set; }
        public int ReviewedToday { get; set; }
        public int TotalReviews { get; set; }
        public double AverageQuality { get; set; }
        public double RetentionRate { get; set; }
        public int CurrentStreak { get; set; }
    }
}