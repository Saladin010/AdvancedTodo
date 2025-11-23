using AdvancedTodoLearningCards.Models;

namespace AdvancedTodoLearningCards.Repositories
{
    public interface IReviewLogRepository : IRepository<ReviewLog>
    {
        Task<IEnumerable<ReviewLog>> GetReviewsByUserIdAsync(string userId, int limit = 50);
        Task<IEnumerable<ReviewLog>> GetReviewsByCardIdAsync(int cardId);
        Task<IEnumerable<ReviewLog>> GetReviewsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task<Dictionary<DateTime, int>> GetReviewCountsByDateAsync(string userId, int days = 30);
        Task<double> GetAverageQualityAsync(string userId);
        Task<int> GetTotalReviewsCountAsync(string userId);
    }
}