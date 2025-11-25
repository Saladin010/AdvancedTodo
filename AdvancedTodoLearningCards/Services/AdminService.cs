using AdvancedTodoLearningCards.Data;
using AdvancedTodoLearningCards.Models;
using AdvancedTodoLearningCards.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AdvancedTodoLearningCards.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardStatisticsAsync()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = today.AddDays(-7);

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalCards = await _context.Cards.CountAsync(),
                TotalReviews = await _context.ReviewLogs.CountAsync(),
                ActiveUsersToday = await _context.ReviewLogs
                    .Where(r => r.ReviewedAt.Date == today)
                    .Select(r => r.UserId)
                    .Distinct()
                    .CountAsync(),
                ActiveUsersThisWeek = await _context.ReviewLogs
                    .Where(r => r.ReviewedAt.Date >= weekAgo)
                    .Select(r => r.UserId)
                    .Distinct()
                    .CountAsync(),
                CardsCreatedToday = await _context.Cards
                    .Where(c => c.CreatedAt.Date == today)
                    .CountAsync(),
                ReviewsCompletedToday = await _context.ReviewLogs
                    .Where(r => r.ReviewedAt.Date == today)
                    .CountAsync(),
                DailyReviews = await GetDailyReviewsAsync(30),
                DailyNewCards = await GetDailyNewCardsAsync(30),
                CardsByDifficulty = await GetCardsByDifficultyAsync(),
                RecentActivity = await GetRecentActivityAsync(20)
            };

            var userCount = viewModel.TotalUsers > 0 ? viewModel.TotalUsers : 1;
            viewModel.AverageCardsPerUser = Math.Round((double)viewModel.TotalCards / userCount, 2);
            viewModel.AverageReviewsPerUser = Math.Round((double)viewModel.TotalReviews / userCount, 2);

            return viewModel;
        }

        public async Task<UserManagementViewModel> GetUserStatisticsAsync(string? searchTerm = null)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u => u.UserName!.Contains(searchTerm) || 
                                        (u.Email != null && u.Email.Contains(searchTerm)));
            }

            var users = await query
                .Select(u => new UserStatistics
                {
                    UserId = u.Id,
                    UserName = u.UserName ?? "Unknown",
                    Email = u.Email,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    CardCount = u.Cards.Count,
                    ReviewCount = u.ReviewLogs.Count,
                    ReviewsToday = u.ReviewLogs.Count(r => r.ReviewedAt.Date == DateTime.UtcNow.Date),
                    AverageQuality = u.ReviewLogs.Any() ? u.ReviewLogs.Average(r => r.Quality) : 0,
                    DaysActive = u.ReviewLogs
                        .Select(r => r.ReviewedAt.Date)
                        .Distinct()
                        .Count()
                })
                .OrderByDescending(u => u.LastLoginAt)
                .ToListAsync();

            return new UserManagementViewModel
            {
                Users = users,
                SearchTerm = searchTerm
            };
        }

        public async Task<List<DailyStatistic>> GetDailyReviewsAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);

            var reviews = await _context.ReviewLogs
                .Where(r => r.ReviewedAt.Date >= startDate)
                .GroupBy(r => r.ReviewedAt.Date)
                .Select(g => new DailyStatistic
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            // Fill in missing dates with zero counts
            var allDates = Enumerable.Range(0, days)
                .Select(i => startDate.AddDays(i))
                .ToList();

            var result = allDates.Select(date => new DailyStatistic
            {
                Date = date,
                Count = reviews.FirstOrDefault(r => r.Date == date)?.Count ?? 0
            }).ToList();

            return result;
        }

        public async Task<List<DailyStatistic>> GetDailyNewCardsAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);

            var cards = await _context.Cards
                .Where(c => c.CreatedAt.Date >= startDate)
                .GroupBy(c => c.CreatedAt.Date)
                .Select(g => new DailyStatistic
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            // Fill in missing dates with zero counts
            var allDates = Enumerable.Range(0, days)
                .Select(i => startDate.AddDays(i))
                .ToList();

            var result = allDates.Select(date => new DailyStatistic
            {
                Date = date,
                Count = cards.FirstOrDefault(c => c.Date == date)?.Count ?? 0
            }).ToList();

            return result;
        }

        public async Task<Dictionary<string, int>> GetCardsByDifficultyAsync()
        {
            var cards = await _context.Cards
                .GroupBy(c => c.Difficulty)
                .Select(g => new { Difficulty = g.Key, Count = g.Count() })
                .ToListAsync();

            return cards.ToDictionary(
                c => c.Difficulty.ToString(),
                c => c.Count
            );
        }

        public async Task<List<RecentActivityItem>> GetRecentActivityAsync(int count = 20)
        {
            var recentReviews = await _context.ReviewLogs
                .Include(r => r.User)
                .Include(r => r.Card)
                .OrderByDescending(r => r.ReviewedAt)
                .Take(count)
                .Select(r => new RecentActivityItem
                {
                    UserName = r.User.UserName ?? "Unknown",
                    ActivityType = "Review",
                    Description = $"Reviewed card: {r.Card.Title}",
                    Timestamp = r.ReviewedAt
                })
                .ToListAsync();

            return recentReviews;
        }
    }
}
