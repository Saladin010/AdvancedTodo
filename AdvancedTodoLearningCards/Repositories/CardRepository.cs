using AdvancedTodoLearningCards.Data;
using AdvancedTodoLearningCards.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AdvancedTodoLearningCards.Repositories
{
    public class CardRepository : Repository<Card>, ICardRepository
    {
        public CardRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Card>> GetCardsByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Card>> GetCardsByUserIdWithScheduleAsync(string userId)
        {
            return await _dbSet
                .Include(c => c.Schedule)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Card?> GetCardWithScheduleAsync(int cardId)
        {
            return await _dbSet
                .Include(c => c.Schedule)
                .FirstOrDefaultAsync(c => c.Id == cardId);
        }

        public async Task<IEnumerable<Card>> GetCardsDueForReviewAsync(string userId, DateTime date)
        {
            return await _dbSet
                .Include(c => c.Schedule)
                .Where(c => c.UserId == userId &&
                           c.Schedule != null &&
                           c.Schedule.NextReviewAt <= date)
                .OrderBy(c => c.Schedule!.NextReviewAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Card>> SearchCardsAsync(string userId, string searchTerm)
        {
            return await _dbSet
                .Include(c => c.Schedule)
                .Where(c => c.UserId == userId &&
                           (c.Title.Contains(searchTerm) || c.Content.Contains(searchTerm)))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Card>> GetCardsByTagsAsync(string userId, string[] tags)
        {
            return await _dbSet
                .Include(c => c.Schedule)
                .Where(c => c.UserId == userId &&
                           c.Tags != null &&
                           tags.Any(tag => c.Tags.Contains(tag)))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetTagsStatisticsAsync(string userId)
        {
            var cards = await _dbSet
                .Where(c => c.UserId == userId && c.Tags != null)
                .Select(c => c.Tags)
                .ToListAsync();

            var tagCounts = new Dictionary<string, int>();

            foreach (var tagsJson in cards)
            {
                if (string.IsNullOrEmpty(tagsJson)) continue;

                try
                {
                    var tags = JsonSerializer.Deserialize<string[]>(tagsJson);
                    if (tags != null)
                    {
                        foreach (var tag in tags)
                        {
                            if (tagCounts.ContainsKey(tag))
                                tagCounts[tag]++;
                            else
                                tagCounts[tag] = 1;
                        }
                    }
                }
                catch { }
            }

            return tagCounts;
        }
    }
}