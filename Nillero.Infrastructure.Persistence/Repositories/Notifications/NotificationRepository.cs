using Microsoft.EntityFrameworkCore;
using Nillero.Core.Domain.Entities.Notifications;
using Nillero.Core.Domain.Interfaces.Notifications;
using Nillero.Infrastructure.Persistence.Contexts;
using Nillero.Infrastructure.Persistence.Repositories.Base;

namespace Nillero.Infrastructure.Persistence.Repositories.Notifications
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly NilleroAppContext _context;

        public NotificationRepository(NilleroAppContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }
    }
}
