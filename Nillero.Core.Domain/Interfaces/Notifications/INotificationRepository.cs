using Nillero.Core.Domain.Entities.Notifications;
using Nillero.Core.Domain.Interfaces.Base;

namespace Nillero.Core.Domain.Interfaces.Notifications
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<int> GetUnreadCountAsync(string userId);
    }
}
