using AdvancedTodoLearningCards.Models;

namespace AdvancedTodoLearningCards.Repositories
{
    public interface ICardRepository : IRepository<Card>
    {
        Task<IEnumerable<Card>> GetCardsByUserIdAsync(string userId);
        Task<IEnumerable<Card>> GetCardsByUserIdWithScheduleAsync(string userId);
        Task<Card?> GetCardWithScheduleAsync(int cardId);
        Task<IEnumerable<Card>> GetCardsDueForReviewAsync(string userId, DateTime date);
        Task<IEnumerable<Card>> SearchCardsAsync(string userId, string searchTerm);
        Task<IEnumerable<Card>> GetCardsByTagsAsync(string userId, string[] tags);
        Task<Dictionary<string, int>> GetTagsStatisticsAsync(string userId);
    }
}