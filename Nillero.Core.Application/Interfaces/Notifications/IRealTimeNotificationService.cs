namespace Nillero.Core.Application.Interfaces.Notifications
{
    public interface IRealTimeNotificationService
    {
        Task SendNotificationAsync(string userId, string message, string iconClass, int unreadCount);
    }
}
