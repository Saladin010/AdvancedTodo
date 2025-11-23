using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AdvancedTodoLearningCards.Hubs
{
    [Authorize]
    public class ReviewNotificationHub : Hub
    {
        private readonly ILogger<ReviewNotificationHub> _logger;

        public ReviewNotificationHub(ILogger<ReviewNotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation($"User {userId} connected to notification hub");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation($"User {userId} disconnected from notification hub");
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client can call this to acknowledge a notification
        /// </summary>
        public async Task AcknowledgeNotification(int notificationId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"User {userId} acknowledged notification {notificationId}");
            
            // The actual database update will be handled by the controller/service
            // This is just for logging and potential real-time updates
            await Task.CompletedTask;
        }
    }
}
