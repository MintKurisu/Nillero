namespace Nillero.Core.Application.ViewModels.Social.FriendRequest
{
    public class PendingFriendRequestViewModel
    {
        public int Id { get; set; }
        public required string SenderId { get; set; }
        public required string SenderUserName { get; set; }
        public required string SenderFullName { get; set; }
        public string? SenderProfilePicture { get; set; }
        public int MutualFriendsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
