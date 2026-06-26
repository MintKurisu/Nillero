using Nillero.Core.Domain.Common.Enum.Social;

namespace Nillero.Core.Domain.Entities.Social
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public required string SenderId { get; set; }
        public required string ReceiverId { get; set; }
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }

    }
}
