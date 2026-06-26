using Nillero.Core.Application.ViewModels.Social.Posts;

namespace Nillero.Core.Application.ViewModels.Social.Friendship
{
    public class FriendsPageViewModel
    {
        public List<FriendViewModel> Friends { get; set; } = new();
        public List<PostViewModel> FriendsPosts { get; set; } = new();
    }
}
