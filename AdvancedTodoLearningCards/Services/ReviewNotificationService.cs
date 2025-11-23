using AdvancedTodoLearningCards.Data;
using AdvancedTodoLearningCards.Hubs;
using AdvancedTodoLearningCards.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AdvancedTodoLearningCards.Services
{
    public class ReviewNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReviewNotificationService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes

        public ReviewNotificationService(
            IServiceProvider serviceProvider,
            ILogger<ReviewNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Review Notification Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndNotifyDueReviews(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Review Notification Service");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Review Notification Service stopped");
        }

        private async Task CheckAndNotifyDueReviews(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ReviewNotificationHub>>();

            var now = DateTime.UtcNow;

            // Find all cards that are due for review and haven't been notified yet
            var dueCards = await context.Cards
                .Include(c => c.Schedule)
                .Where(c => c.Schedule != null && 
                           c.Schedule.NextReviewAt <= now &&
                           !context.ReviewNotifications.Any(rn => 
                               rn.CardId == c.Id && 
                               rn.ScheduledReviewAt == c.Schedule.NextReviewAt))
                .ToListAsync(cancellationToken);

            if (dueCards.Any())
            {
                _logger.LogInformation($"Found {dueCards.Count} cards due for review");

                foreach (var card in dueCards)
                {
                    // Create notification record
                    var notification = new ReviewNotification
                    {
                        UserId = card.UserId,
                        CardId = card.Id,
                        NotifiedAt = DateTime.UtcNow,
                        ScheduledReviewAt = card.Schedule!.NextReviewAt,
                        IsAcknowledged = false
                    };

                    context.ReviewNotifications.Add(notification);
                    await context.SaveChangesAsync(cancellationToken);

                    // Send real-time notification via SignalR
                    try
                    {
                        await hubContext.Clients
                            .Group($"user_{card.UserId}")
                            .SendAsync("ReceiveNotification", new
                            {
                                notificationId = notification.Id,
                                cardId = card.Id,
                                cardTitle = card.Title,
                                scheduledAt = card.Schedule.NextReviewAt,
                                notifiedAt = notification.NotifiedAt
                            }, cancellationToken);

                        _logger.LogInformation($"Sent notification for card {card.Id} to user {card.UserId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending notification for card {card.Id}");
                    }
                }
            }
        }
    }
}
