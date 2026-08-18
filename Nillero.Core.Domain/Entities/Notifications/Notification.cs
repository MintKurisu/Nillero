using Nillero.Core.Domain.Common.Enum;
using System.ComponentModel.DataAnnotations;
namespace Nillero.Core.Domain.Entities.Notifications
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;          
        public string ActorUserId { get; set; } = null!;     
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
    }

}
