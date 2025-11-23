using AdvancedTodoLearningCards.Models;
using AdvancedTodoLearningCards.Repositories;
using System.Text.Json;

namespace AdvancedTodoLearningCards.Services
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IRepository<CardSchedule> _scheduleRepository;
        private readonly ISchedulingEngine _schedulingEngine;
        private readonly ILogger<CardService> _logger;

        public CardService(
            ICardRepository cardRepository,
            IRepository<CardSchedule> scheduleRepository,
            ISchedulingEngine schedulingEngine,
            ILogger<CardService> logger)
        {
            _cardRepository = cardRepository;
            _scheduleRepository = scheduleRepository;
            _schedulingEngine = schedulingEngine;
            _logger = logger;
        }

        public async Task<IEnumerable<Card>> GetAllCardsAsync(string userId)
        {
            return await _cardRepository.GetCardsByUserIdWithScheduleAsync(userId);
        }

        public async Task<Card?> GetCardByIdAsync(int cardId, string userId)
        {
            var card = await _cardRepository.GetCardWithScheduleAsync(cardId);

            if (card == null || card.UserId != userId)
                return null;

            return card;
        }

        public async Task<Card> CreateCardAsync(Card card)
        {
            card.CreatedAt = DateTime.UtcNow;
            card.UpdatedAt = DateTime.UtcNow;

            await _cardRepository.AddAsync(card);
            await _cardRepository.SaveChangesAsync();

            // Create initial schedule
            var schedule = _schedulingEngine.InitializeSchedule(card.Id);
            await _scheduleRepository.AddAsync(schedule);
            await _scheduleRepository.SaveChangesAsync();

            _logger.LogInformation($"Card created: ID={card.Id}, User={card.UserId}");

            return card;
        }

        public async Task<Card> UpdateCardAsync(Card card)
        {
            card.UpdatedAt = DateTime.UtcNow;
            _cardRepository.Update(card);
            await _cardRepository.SaveChangesAsync();

            _logger.LogInformation($"Card updated: ID={card.Id}");

            return card;
        }

        public async Task<bool> DeleteCardAsync(int cardId, string userId)
        {
            var card = await _cardRepository.GetCardWithScheduleAsync(cardId);

            if (card == null || card.UserId != userId)
                return false;

            _cardRepository.Remove(card);
            await _cardRepository.SaveChangesAsync();

            _logger.LogInformation($"Card deleted: ID={cardId}");

            return true;
        }

        public async Task<IEnumerable<Card>> SearchCardsAsync(string userId, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCardsAsync(userId);

            return await _cardRepository.SearchCardsAsync(userId, searchTerm);
        }

        public async Task<IEnumerable<Card>> GetCardsByTagAsync(string userId, string tag)
        {
            return await _cardRepository.GetCardsByTagsAsync(userId, new[] { tag });
        }

        public async Task<Dictionary<string, int>> GetTagsStatisticsAsync(string userId)
        {
            return await _cardRepository.GetTagsStatisticsAsync(userId);
        }

        public async Task<int> ImportCardsFromCsvAsync(string userId, string csvContent)
        {
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1) return 0;

            var cards = new List<Card>();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                if (values.Length < 2) continue;

                var card = new Card
                {
                    UserId = userId,
                    Title = values[0].Trim(),
                    Content = values[1].Trim(),
                    Tags = values.Length > 2 ? JsonSerializer.Serialize(values[2].Split(';')) : null,
                    Difficulty = values.Length > 3 && Enum.TryParse<CardDifficulty>(values[3].Trim(), out var diff)
                        ? diff
                        : CardDifficulty.Medium,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                cards.Add(card);
            }

            await _cardRepository.AddRangeAsync(cards);
            await _cardRepository.SaveChangesAsync();

            // Create schedules for all imported cards
            foreach (var card in cards)
            {
                var schedule = _schedulingEngine.InitializeSchedule(card.Id);
                await _scheduleRepository.AddAsync(schedule);
            }
            await _scheduleRepository.SaveChangesAsync();

            _logger.LogInformation($"Imported {cards.Count} cards for user {userId}");

            return cards.Count;
        }
    }
}