using AdvancedTodoLearningCards.Repositories;

namespace AdvancedTodoLearningCards.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IReviewLogRepository _reviewLogRepository;
        private readonly IReviewService _reviewService;

        public DashboardService(
            ICardRepository cardRepository,
            IReviewLogRepository reviewLogRepository,
            IReviewService reviewService)
        {
            _cardRepository = cardRepository;
            _reviewLogRepository = reviewLogRepository;
            _reviewService = reviewService;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync(string userId)
        {
            var totalCards = await _cardRepository.CountAsync(c => c.UserId == userId);
            var dueToday = await _reviewService.GetDueCountAsync(userId);
            var totalReviews = await _reviewLogRepository.GetTotalReviewsCountAsync(userId);
            var averageQuality = await _reviewLogRepository.GetAverageQualityAsync(userId);
            var retentionRate = await GetRetentionRateAsync(userId);

            // Get reviews today
            var today = DateTime.UtcNow.Date;
            var reviewsToday = await _reviewLogRepository.FindAsync(r =>
                r.UserId == userId && r.ReviewedAt.Date == today);

            // Calculate current streak
            var streak = await CalculateCurrentStreakAsync(userId);

            return new DashboardStats
            {
                TotalCards = totalCards,
                DueToday = dueToday,
                ReviewedToday = reviewsToday.Count(),
                TotalReviews = totalReviews,
                AverageQuality = averageQuality,
                RetentionRate = retentionRate,
                CurrentStreak = streak
            };
        }

        public async Task<Dictionary<DateTime, int>> GetReviewChartDataAsync(string userId, int days = 30)
        {
            return await _reviewLogRepository.GetReviewCountsByDateAsync(userId, days);
        }

        public async Task<Dictionary<string, int>> GetTagPerformanceAsync(string userId)
        {
            return await _cardRepository.GetTagsStatisticsAsync(userId);
        }

        public async Task<Dictionary<string, int>> GetDifficultyDistributionAsync(string userId)
        {
            var cards = await _cardRepository.GetCardsByUserIdAsync(userId);

            return cards
                .GroupBy(c => c.Difficulty.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<double> GetRetentionRateAsync(string userId)
        {
            var reviews = await _reviewLogRepository.FindAsync(r => r.UserId == userId);

            if (!reviews.Any())
                return 0;

            var successfulReviews = reviews.Count(r => r.Quality >= 3);
            return (double)successfulReviews / reviews.Count() * 100;
        }

        private async Task<int> CalculateCurrentStreakAsync(string userId)
        {
            var reviewsByDate = await _reviewLogRepository.GetReviewCountsByDateAsync(userId, 365);

            if (!reviewsByDate.Any())
                return 0;

            var streak = 0;
            var currentDate = DateTime.UtcNow.Date;

            while (reviewsByDate.ContainsKey(currentDate))
            {
                streak++;
                currentDate = currentDate.AddDays(-1);
            }

            return streak;
        }
    }
}