using FluentValidation;
using Nillero.Core.Application.ViewModels.Social.Comment;

namespace Nillero.Core.Application.Validators.Social.Comment
{
    public class SaveCommentViewModelValidator : AbstractValidator<SaveCommentViewModel>
    {
        public SaveCommentViewModelValidator()
        {
            RuleFor(x => x.PostId)
                .GreaterThan(0).WithMessage("A valid Post ID is required.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.")
                .MaximumLength(2000).WithMessage("Comment cannot exceed 2000 characters.");
        }
    }
}
