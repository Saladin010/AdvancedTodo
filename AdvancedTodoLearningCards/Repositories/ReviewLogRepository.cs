using AdvancedTodoLearningCards.Data;
using AdvancedTodoLearningCards.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedTodoLearningCards.Repositories
{
    public class ReviewLogRepository : Repository<ReviewLog>, IReviewLogRepository
    {
        public ReviewLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ReviewLog>> GetReviewsByUserIdAsync(string userId, int limit = 50)
        {
            return await _dbSet
                .Include(r => r.Card)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReviewedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewLog>> GetReviewsByCardIdAsync(int cardId)
        {
            return await _dbSet
                .Where(r => r.CardId == cardId)
                .OrderByDescending(r => r.ReviewedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewLog>> GetReviewsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(r => r.Card)
                .Where(r => r.UserId == userId &&
                           r.ReviewedAt >= startDate &&
                           r.ReviewedAt <= endDate)
                .OrderByDescending(r => r.ReviewedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<DateTime, int>> GetReviewCountsByDateAsync(string userId, int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var reviews = await _dbSet
                .Where(r => r.UserId == userId && r.ReviewedAt >= startDate)
                .Select(r => r.ReviewedAt.Date)
                .ToListAsync();

            return reviews
                .GroupBy(date => date)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<double> GetAverageQualityAsync(string userId)
        {
            var reviews = await _dbSet
                .Where(r => r.UserId == userId)
                .Select(r => r.Quality)
                .ToListAsync();

            return reviews.Any() ? reviews.Average() : 0;
        }

        public async Task<int> GetTotalReviewsCountAsync(string userId)
        {
            return await _dbSet.CountAsync(r => r.UserId == userId);
        }
    }
}