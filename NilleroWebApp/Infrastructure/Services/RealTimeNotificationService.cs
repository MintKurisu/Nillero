using Microsoft.AspNetCore.SignalR;
using Nillero.Core.Application.Interfaces.Notifications;
using NilleroWebApp.Hubs;

namespace NilleroWebApp.Infrastructure.Services
{
    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<RealTimeNotificationService> _logger;

        public RealTimeNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<RealTimeNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }
        public async Task SendNotificationAsync(
            string userId,
            string message,
            string iconClass,
            int unreadCount)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning(
                    "[RealTimeNotificationService] SendNotificationAsync called with null or empty userId. Skipping push.");
                return;
            }

            try
            {
                // The hub registers each user under "user:{userId}" in OnConnectedAsync.
                // Sending to the group reaches every open tab the user has.
                string groupName = $"user:{userId}";

                await _hubContext.Clients
                    .Group(groupName)
                    .SendAsync("ReceiveNotification", new
                    {
                        message = message,
                        iconClass = iconClass,
                        unreadCount = unreadCount
                    });

                _logger.LogDebug(
                    "[RealTimeNotificationService] Pushed notification to group '{Group}': '{Message}' (unread: {Count}).",
                    groupName, message, unreadCount);
            }
            catch (Exception ex)
            {
                // Log and swallow — a SignalR failure must never propagate up
                // and interrupt the calling service's primary business logic.
                _logger.LogError(
                    ex,
                    "[RealTimeNotificationService] Failed to push real-time notification to user '{UserId}'. " +
                    "The notification was already persisted to the database. SignalR error: {Error}",
                    userId, ex.Message);
            }
        }
    }
}
