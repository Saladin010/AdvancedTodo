using AdvancedTodoLearningCards.Models;

namespace AdvancedTodoLearningCards.Services
{
    public interface ICardService
    {
        Task<IEnumerable<Card>> GetAllCardsAsync(string userId);
        Task<Card?> GetCardByIdAsync(int cardId, string userId);
        Task<Card> CreateCardAsync(Card card);
        Task<Card> UpdateCardAsync(Card card);
        Task<bool> DeleteCardAsync(int cardId, string userId);
        Task<IEnumerable<Card>> SearchCardsAsync(string userId, string searchTerm);
        Task<IEnumerable<Card>> GetCardsByTagAsync(string userId, string tag);
        Task<Dictionary<string, int>> GetTagsStatisticsAsync(string userId);
        Task<int> ImportCardsFromCsvAsync(string userId, string csvContent);
    }
}