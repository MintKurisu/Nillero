using FluentValidation;
using Nillero.Core.Application.ViewModels.Social.Posts;

namespace Nillero.Core.Application.Validators.Social.Posts
{
    public class SavePostViewModelValidator : AbstractValidator<SavePostViewModel>
    {
        public SavePostViewModelValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.")
                .MaximumLength(5000).WithMessage("Content cannot exceed 5000 characters.");

            RuleFor(x => x.MediaType)
                .NotEmpty().WithMessage("You must select a publication type.")
                .Must(x => x == "Image" || x == "Video")
                .WithMessage("Publication type must be Image or Video.");

            When(x => x.MediaType == "Image" && !x.Id.HasValue, () =>
            {
                RuleFor(x => x.ImageFile)
                    .NotNull().WithMessage("You must upload an image.");
            });

            When(x => x.MediaType == "Video", () =>
            {
                RuleFor(x => x.YouTubeUrl)
                    .NotEmpty().WithMessage("You must provide a YouTube link.")
                    .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                    .WithMessage("The YouTube URL is not valid.");
            });
        }
    }
}
