namespace Nillero.Core.Application.ViewModels.Social.FriendRequest
{
    public class FriendRequestsPageViewModel
    {
        public List<PendingFriendRequestViewModel> PendingRequests { get; set; } = new();
        public List<SentFriendRequestViewModel> SentRequests { get; set; } = new();
        public int PendingCount { get; set; }
    }
}
