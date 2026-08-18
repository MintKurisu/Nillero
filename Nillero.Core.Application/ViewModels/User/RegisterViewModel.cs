using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Nillero.Core.Application.ViewModels.Login
{
    public class RegisterViewModel
    {
        [Display(Name = "First Name")]
        public required string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public required string LastName { get; set; }

        [Display(Name = "Phone")]
        public required string Phone { get; set; }

        [Display(Name = "Email Address")]
        public required string Email { get; set; }

        public string? ProfilePicturePath { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }

        [Display(Name = "Username")]
        public required string UserName { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        public required string ConfirmPassword { get; set; }
    }

}
