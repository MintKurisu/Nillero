namespace Nillero.Core.Application.Dtos.User
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public string? ProfilePicturePath { get; set; }
        public required string Role { get; set; }
        public bool? IsVerified { get; set; }
        public bool IsActive { get; set; } = false;

       
    }
}
