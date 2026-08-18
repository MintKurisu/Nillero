using FluentValidation;
using Nillero.Core.Application.ViewModels.Login.Password;

namespace Nillero.Core.Application.Validators.User
{
    public class ResetPasswordViewModelValidator : AbstractValidator<ResetPasswordViewModel>
    {
        public ResetPasswordViewModelValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User identity is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Length(6, 100).WithMessage("Password must be between 6 and 100 characters.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("You must confirm the password.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}
