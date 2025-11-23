using AdvancedTodoLearningCards.Models;
using AdvancedTodoLearningCards.Repositories;
using AdvancedTodoLearningCards.Data;
using Microsoft.EntityFrameworkCore;

namespace AdvancedTodoLearningCards.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IRepository<CardSchedule> _scheduleRepository;
        private readonly IReviewLogRepository _reviewLogRepository;
        private readonly ISchedulingEngine _fixedSchedulingEngine;
        private readonly Sm2SchedulingEngine _sm2SchedulingEngine;
        private readonly ILogger<ReviewService> _logger;
        private readonly ApplicationDbContext _context;

        public ReviewService(
            ICardRepository cardRepository,
            IRepository<CardSchedule> scheduleRepository,
            IReviewLogRepository reviewLogRepository,
            ISchedulingEngine fixedSchedulingEngine,
            Sm2SchedulingEngine sm2SchedulingEngine,
            ILogger<ReviewService> logger,
            ApplicationDbContext context)
        {
            _cardRepository = cardRepository;
            _scheduleRepository = scheduleRepository;
            _reviewLogRepository = reviewLogRepository;
            _fixedSchedulingEngine = fixedSchedulingEngine;
            _sm2SchedulingEngine = sm2SchedulingEngine;
            _logger = logger;
            _context = context;
        }

        public async Task<IEnumerable<Card>> GetDueCardsAsync(string userId)
        {
            return await _cardRepository.GetCardsDueForReviewAsync(userId, DateTime.UtcNow);
        }

        public async Task<IEnumerable<Card>> GetUpcomingReviewsAsync(string userId, int days = 7)
        {
            var endDate = DateTime.UtcNow.AddDays(days);
            var allCards = await _cardRepository.GetCardsByUserIdWithScheduleAsync(userId);

            return allCards
                .Where(c => c.Schedule != null &&
                           c.Schedule.NextReviewAt > DateTime.UtcNow &&
                           c.Schedule.NextReviewAt <= endDate)
                .OrderBy(c => c.Schedule!.NextReviewAt)
                .ToList();
        }

        public async Task<ReviewLog> ProcessReviewAsync(int cardId, string userId, int quality, int? timeSpentSeconds = null)
        {
            // Get card with schedule
            var card = await _cardRepository.GetCardWithScheduleAsync(cardId);

            if (card == null || card.UserId != userId || card.Schedule == null)
                throw new InvalidOperationException("Card not found or does not belong to user");

            var schedule = card.Schedule;

            // Store old values for logging
            var oldInterval = schedule.IntervalDays;
            var oldEF = schedule.EaseFactor;

            // Choose appropriate scheduling engine
            ISchedulingEngine engine = schedule.SchedulingMode == SchedulingMode.Fixed
                ? _fixedSchedulingEngine
                : _sm2SchedulingEngine;

            // Calculate next review
            schedule = engine.CalculateNextReview(schedule, quality);

            // Update schedule in database
            _scheduleRepository.Update(schedule);
            await _scheduleRepository.SaveChangesAsync();

            // Create review log
            var reviewLog = new ReviewLog
            {
                CardId = cardId,
                UserId = userId,
                ReviewedAt = DateTime.UtcNow,
                Quality = quality,
                IntervalBefore = oldInterval,
                IntervalAfter = schedule.IntervalDays,
                EaseFactorBefore = oldEF,
                EaseFactorAfter = schedule.EaseFactor,
                TimeSpentSeconds = timeSpentSeconds
            };

            await _reviewLogRepository.AddAsync(reviewLog);
            await _reviewLogRepository.SaveChangesAsync();

            _logger.LogInformation(
                $"Review processed: Card={cardId}, Quality={quality}, OldInterval={oldInterval}, NewInterval={schedule.IntervalDays}");

            return reviewLog;
        }

        public async Task<IEnumerable<ReviewLog>> GetReviewHistoryAsync(string userId, int limit = 50)
        {
            return await _reviewLogRepository.GetReviewsByUserIdAsync(userId, limit);
        }

        public async Task<Dictionary<DateTime, int>> GetReviewStatisticsAsync(string userId, int days = 30)
        {
            return await _reviewLogRepository.GetReviewCountsByDateAsync(userId, days);
        }

        public async Task<int> GetDueCountAsync(string userId)
        {
            var dueCards = await GetDueCardsAsync(userId);
            return dueCards.Count();
        }

        // Notification management methods
        public async Task<IEnumerable<ReviewNotification>> GetUnacknowledgedNotificationsAsync(string userId)
        {
            return await _context.ReviewNotifications
                .Include(rn => rn.Card)
                .Where(rn => rn.UserId == userId && !rn.IsAcknowledged)
                .OrderByDescending(rn => rn.NotifiedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnacknowledgedCountAsync(string userId)
        {
            return await _context.ReviewNotifications
                .CountAsync(rn => rn.UserId == userId && !rn.IsAcknowledged);
        }

        public async Task AcknowledgeNotificationAsync(int notificationId, string userId)
        {
            var notification = await _context.ReviewNotifications
                .FirstOrDefaultAsync(rn => rn.Id == notificationId && rn.UserId == userId);

            if (notification != null && !notification.IsAcknowledged)
            {
                notification.IsAcknowledged = true;
                notification.AcknowledgedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Notification {notificationId} acknowledged by user {userId}");
            }
        }

        public async Task<ReviewNotification> CreateNotificationAsync(string userId, int cardId, DateTime scheduledReviewAt)
        {
            var notification = new ReviewNotification
            {
                UserId = userId,
                CardId = cardId,
                NotifiedAt = DateTime.UtcNow,
                ScheduledReviewAt = scheduledReviewAt,
                IsAcknowledged = false
            };

            _context.ReviewNotifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }
    }
}