using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Nillero.Core.Application.ViewModels.Social.User
{
    public class EditProfileViewModel
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
        [Phone(ErrorMessage = "The phone format is not valid.")]
        [Display(Name = "Phone")]
        public required string Phone { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }

        public string? CurrentProfilePicturePath { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "New Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }
    }
}
