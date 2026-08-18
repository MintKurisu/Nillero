using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nillero.Core.Application.Dtos.User;
using Nillero.Core.Application.Interfaces.Presentation.Mappers;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Interfaces.User;
using Nillero.Core.Application.ViewModels.Search;
using Nillero.Infrastructure.Identity.Entities;
using NilleroWebApp.Controllers;
using Supabase.Gotrue;

[Authorize]
public class SearchController : BaseController
{
    private readonly IPostService _postService;
    private readonly IAccountServicesForWebApp _accountService;
    private readonly IPostViewModelMapper _postViewModelMapper;
    private readonly IMapper _mapper;

    public SearchController(
        IPostService postService,
        IAccountServicesForWebApp accountService,
        IPostViewModelMapper postViewModelMapper,
        UserManager<ApplicationUser> userManager,
        IMapper mapper) : base(userManager)
    {
        _postService = postService;
        _accountService = accountService;
        _postViewModelMapper = postViewModelMapper;
        _mapper = mapper;
    }

    // GET /Search?q=term  → full results page (Enter key)
    public async Task<IActionResult> Index(string? q)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return RedirectToAction("Index", "Login");

        ViewData["ActiveNav"] = "search";
        ViewData["UserAvatarUrl"] = currentUser.ProfilePicturePath;
        ViewData["UserHandle"] = currentUser.UserName;

        if (string.IsNullOrWhiteSpace(q))
            return View(new SearchResultViewModel { Query = string.Empty });

        var vm = await BuildSearchResultAsync(q, currentUser, maxResults: 20);
        return View(vm);
    }

    // GET /Search/Live?q=term  → JSON for the live dropdown (fetch)
    [HttpGet]
    public async Task<IActionResult> Live(string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(new { people = Array.Empty<object>(), posts = Array.Empty<object>() });

        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return Unauthorized();

        var vm = await BuildSearchResultAsync(q, currentUser, maxResults: 4);

        return Json(new
        {
            people = vm.People.Select(p => new
            {
                userId = p.UserId,
                userName = p.UserName,
                fullName = p.FullName,
                avatar = p.ProfilePicture
            }),
            posts = vm.Posts.Select(p => new
            {
                postId = p.PostId,
                authorName = p.AuthorFullName,
                authorHandle = p.AuthorUserName,
                avatar = p.AuthorProfilePicture,
                snippet = p.ContentSnippet,
                createdAt = p.CreatedAt.ToString("MMM d")
            })
        });
    }

    #region Private

    private async Task<SearchResultViewModel> BuildSearchResultAsync(
        string q,
        ApplicationUser currentUser,
        int maxResults)
    {
        var term = q.Trim();
        var vm = new SearchResultViewModel { Query = term };

        // ── People ───────────────────────────────────────────
        // GetAllUser already hits UserManager.Users as IQueryable (LIKE on UserName,
        // FirstName, LastName once you apply the filter expansion from the previous step).
        // Take(maxResults) before the role-fetching foreach to avoid N+1 at scale.
        var allUsers = await _accountService.GetAllUser(true, term);
        var topUsers = allUsers
            .Where(u => u.Id != currentUser.Id) // exclude self from results
            .Take(maxResults)
            .ToList();

        vm.People = topUsers.Select(u => new UserSearchItemViewModel
        {
            UserId = u.Id,
            UserName = u.UserName,
            FullName = $"{u.FirstName} {u.LastName}",
            ProfilePicture = u.ProfilePicturePath
        }).ToList();

        // ── Posts (friends only) ─────────────────────────────
        // Reuses the existing GetFriendsPostsAsync query (already joins friendships).
        // Filter in-memory after fetch — avoids adding a new repo method for now.
        // At higher scale, promote this to a dedicated IQueryable method with EF Core.
        var friendPosts = await _postService.GetFriendsPostsAsync(currentUser.Id);

        var matchingPosts = friendPosts
            .Where(p => p.Content.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.CreatedAt)
            .Take(maxResults)
            .ToList();

        // Resolve authors — collect distinct userIds first to minimize UserManager calls
        var authorIds = matchingPosts.Select(p => p.UserId).Distinct().ToList();
        var authors = new Dictionary<string, UserDto>();

        foreach (var id in authorIds)
        {
            var author = await _accountService.GetUserById(id);
            if (author != null) authors[id] = author;
        }

        vm.Posts = matchingPosts.Select(p =>
        {
            authors.TryGetValue(p.UserId, out var author);
            return new PostSearchItemViewModel
            {
                PostId = p.Id,
                AuthorUserName = author?.UserName ?? string.Empty,
                AuthorFullName = author != null
                                           ? $"{author.FirstName} {author.LastName}"
                                           : string.Empty,
                AuthorProfilePicture = author?.ProfilePicturePath,
                ContentSnippet = BuildSnippet(p.Content, term, 120),
                CreatedAt = p.CreatedAt
            };
        }).ToList();

        // PostViewModels is only needed by the full results page (Index action),
        // not by the Live dropdown. IPostViewModelMapper handles reaction state,
        // IsOwner, comment tree — same as Friends/Index does.
        vm.PostViewModels = await _postViewModelMapper.MapAsync(matchingPosts, currentUser.Id);

        return vm;
    }

    // Returns a ~120-char excerpt centered around the first match of `term`
    private static string BuildSnippet(string content, string term, int maxLength)
    {
        var idx = content.IndexOf(term, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return content.Length <= maxLength ? content : content[..maxLength] + "…";

        var start = Math.Max(0, idx - 40);
        var raw = content.Substring(start, Math.Min(maxLength, content.Length - start));
        var result = start > 0 ? "…" + raw : raw;
        return result.Length < content.Length - start ? result + "…" : result;
    }

    #endregion
}