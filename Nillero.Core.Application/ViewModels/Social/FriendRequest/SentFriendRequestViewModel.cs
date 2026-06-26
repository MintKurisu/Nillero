using Nillero.Core.Domain.Common.Enum.Social;

namespace Nillero.Core.Application.ViewModels.Social.FriendRequest
{
    public class SentFriendRequestViewModel
    {
        public int Id { get; set; }
        public required string ReceiverId { get; set; }
        public required string ReceiverUserName { get; set; }
        public required string ReceiverFullName { get; set; }
        public string? ReceiverProfilePicture { get; set; }
        public int MutualFriendsCount { get; set; }
        public required FriendRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
