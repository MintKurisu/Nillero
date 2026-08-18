using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Presentation.Mappers;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.ViewModels.Social.Comment;
using Nillero.Core.Application.ViewModels.Social.Posts;
using Nillero.Infrastructure.Identity.Entities;

namespace Nillero.Infrastructure.Shared.Services.Presentation
{
    public class PostViewModelMapper : IPostViewModelMapper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICommentService _commentService;
        private readonly IPostReactionService _postReactionService;
        private readonly IMapper _mapper;

        public PostViewModelMapper(
            UserManager<ApplicationUser> userManager,
            ICommentService commentService,
            IPostReactionService reactionService,
            IMapper mapper)
        {
            _userManager = userManager;
            _commentService = commentService;
            _postReactionService = reactionService;
            _mapper = mapper;
        }

        public async Task<List<PostViewModel>> MapAsync(
            List<PostDto> posts,
            string currentUserId)
        {
            var result = new List<PostViewModel>();

            foreach (var post in posts)
            {
                result.Add(await MapPostAsync(post, currentUserId));
            }

            return result;
        }

        private async Task<PostViewModel> MapPostAsync(
            PostDto postDto,
            string currentUserId)
        {
            var postOwner = await _userManager.FindByIdAsync(postDto.UserId);

            var comments = await _commentService.GetCommentsByPostIdAsync(postDto.Id);

            var (likes, dislikes) =
                await _postReactionService.GetReactionCountsAsync(postDto.Id);

            var userReaction =
                await _postReactionService.GetUserReactionAsync(currentUserId, postDto.Id);

            var postVm = _mapper.Map<PostViewModel>(postDto);

            postVm.UserName = postOwner?.UserName ?? "";
            postVm.UserFullName = $"{postOwner?.FirstName} {postOwner?.LastName}";
            postVm.UserProfilePicture = postOwner?.ProfilePicturePath;
            postVm.IsOwner = postDto.UserId == currentUserId;

            postVm.LikeCount = likes;
            postVm.DislikeCount = dislikes;
            postVm.UserReaction = userReaction;

            postVm.Comments = await MapCommentsAsync(comments, currentUserId);

            return postVm;
        }

        private async Task<List<CommentViewModel>> MapCommentsAsync(
            List<CommentDto> comments,
            string currentUserId)
        {
            var result = new List<CommentViewModel>();

            foreach (var comment in comments)
            {
                result.Add(await MapCommentAsync(comment, currentUserId));
            }

            return result;
        }

        private async Task<CommentViewModel> MapCommentAsync(
            CommentDto commentDto,
            string currentUserId)
        {
            var owner = await _userManager.FindByIdAsync(commentDto.UserId);

            var vm = _mapper.Map<CommentViewModel>(commentDto);

            vm.UserName = owner?.UserName ?? "";
            vm.UserFullName = $"{owner?.FirstName} {owner?.LastName}";
            vm.UserProfilePicture = owner?.ProfilePicturePath;
            vm.IsOwner = commentDto.UserId == currentUserId;

            vm.Replies = await MapCommentsAsync(commentDto.Replies, currentUserId);

            return vm;
        }
    }
}

