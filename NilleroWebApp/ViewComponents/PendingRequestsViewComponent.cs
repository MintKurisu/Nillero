using Nillero.Core.Application.Interfaces.Social;
using Nillero.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NilleroWebApp.ViewComponents
{
    public class PendingRequestsViewComponent : ViewComponent
    {
        private readonly IFriendRequestService _friendRequestService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PendingRequestsViewComponent(
            IFriendRequestService friendRequestService,
            UserManager<ApplicationUser> userManager)
        {
            _friendRequestService = friendRequestService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return View(0);

            var user = await _userManager.GetUserAsync(HttpContext.User);
            int pendingCount = await _friendRequestService.GetPendingRequestsCountAsync(user.Id);

            return View("~/Views/Shared/PendingRequests.cshtml", pendingCount);
        }
    }
}
