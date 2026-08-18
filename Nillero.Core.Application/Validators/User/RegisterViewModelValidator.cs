using FluentValidation;
using Nillero.Core.Application.ViewModels.Login;

namespace Nillero.Core.Application.Validators.User
{
    public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterViewModelValidator()
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

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.ProfilePicture)
                .NotNull().WithMessage("Profile picture is required.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Length(8, 100).WithMessage("Password must be between 8 and 100 characters.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("You must confirm the password.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}
