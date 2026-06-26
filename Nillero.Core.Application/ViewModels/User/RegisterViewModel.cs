using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Nillero.Core.Application.ViewModels.Login
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(60)]
        [Display(Name = "First Name")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(60)]
        [Display(Name = "Last Name")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Format: 809-123-4567")]
        [Display(Name = "Phone")]
        public required string Phone { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Email Address")]
        public required string Email { get; set; }

        public string? ProfilePicturePath { get; set; }

        [Required(ErrorMessage = "Profile picture is required.")]
        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public required string Password { get; set; }

        [Required(ErrorMessage = "You must confirm the password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public required string ConfirmPassword { get; set; }
    }

}
