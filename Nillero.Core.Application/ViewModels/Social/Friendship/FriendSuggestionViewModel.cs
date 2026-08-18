namespace Nillero.Core.Application.ViewModels.Social.Friendship
{
    public class FriendSuggestionViewModel
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? ProfilePicturePath { get; set; }
        public int MutualFriendsCount { get; set; }
    }
}
