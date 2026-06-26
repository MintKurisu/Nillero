namespace Nillero.Core.Application.Dtos.User
{
    public class SaveUserDto
    {
        public string? Id { get; set; }
        public required string UserName { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Phone { get; set; }
        public string? ProfilePicturePath { get; set; }
        public bool IsActive { get; set; } = false; 
        public required string Role { get; set; }

    }
}
