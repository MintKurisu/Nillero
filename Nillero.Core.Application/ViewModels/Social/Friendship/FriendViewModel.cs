namespace Nillero.Core.Application.ViewModels.Social.Friendship
{
    public class FriendViewModel
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime FriendshipCreatedAt { get; set; }
    }
}
