namespace Nillero.Core.Application.Dtos.Social
{
    public class FriendSuggestionDto
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? ProfilePicturePath { get; set; }
        public int MutualFriendsCount { get; set; }
    }
}
