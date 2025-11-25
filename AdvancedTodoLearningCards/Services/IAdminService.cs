using AdvancedTodoLearningCards.ViewModels;

namespace AdvancedTodoLearningCards.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardStatisticsAsync();
        Task<UserManagementViewModel> GetUserStatisticsAsync(string? searchTerm = null);
        Task<List<DailyStatistic>> GetDailyReviewsAsync(int days = 30);
        Task<List<DailyStatistic>> GetDailyNewCardsAsync(int days = 30);
        Task<Dictionary<string, int>> GetCardsByDifficultyAsync();
        Task<List<RecentActivityItem>> GetRecentActivityAsync(int count = 20);
    }
}
