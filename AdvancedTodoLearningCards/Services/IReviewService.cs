using AdvancedTodoLearningCards.Models;

namespace AdvancedTodoLearningCards.Services
{
    /// </summary>
    public interface IReviewService
    {
        Task<IEnumerable<Card>> GetDueCardsAsync(string userId);
        Task<IEnumerable<Card>> GetUpcomingReviewsAsync(string userId, int days = 7);
        Task<ReviewLog> ProcessReviewAsync(int cardId, string userId, int quality, int? timeSpentSeconds = null);
        Task<IEnumerable<ReviewLog>> GetReviewHistoryAsync(string userId, int limit = 50);
        Task<Dictionary<DateTime, int>> GetReviewStatisticsAsync(string userId, int days = 30);
        Task<int> GetDueCountAsync(string userId);

        // Notification management
        Task<IEnumerable<ReviewNotification>> GetUnacknowledgedNotificationsAsync(string userId);
        Task<int> GetUnacknowledgedCountAsync(string userId);
        Task AcknowledgeNotificationAsync(int notificationId, string userId);
        Task<ReviewNotification> CreateNotificationAsync(string userId, int cardId, DateTime scheduledReviewAt);
    }
}