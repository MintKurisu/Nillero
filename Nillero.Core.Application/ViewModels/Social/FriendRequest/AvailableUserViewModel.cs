namespace Nillero.Core.Application.ViewModels.Social.FriendRequest
{
    public class AvailableUserViewModel
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public int MutualFriendsCount { get; set; }
    }
}
