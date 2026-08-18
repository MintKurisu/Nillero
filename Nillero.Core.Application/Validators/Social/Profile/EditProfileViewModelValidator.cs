using FluentValidation;
using Nillero.Core.Application.ViewModels.Social.User;

namespace Nillero.Core.Application.Validators.Social.Profile
{
    public class EditProfileViewModelValidator : AbstractValidator<EditProfileViewModel>
    {
        public EditProfileViewModelValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(60).WithMessage("First name cannot exceed 60 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(60).WithMessage("Last name cannot exceed 60 characters.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required.")
                .Matches(@"^(809|829|849)-\d{3}-\d{4}$")
                .WithMessage("Format must be a valid Dominican number: 809-123-4567");

            When(x => !string.IsNullOrEmpty(x.Password), () =>
            {
                RuleFor(x => x.Password)
                    .Length(6, 100).WithMessage("New password must be between 6 and 100 characters.");

                RuleFor(x => x.ConfirmPassword)
                    .NotEmpty().WithMessage("Please confirm your new password.")
                    .Equal(x => x.Password).WithMessage("Passwords do not match.");
            });
        }
    }
}
