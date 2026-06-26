namespace Nillero.Core.Application.Dtos.Email
{
    public class EmailRequestDto
    {
        public string? ToEmail { get; set; }
        public required string Subject { get; set; }
        public required string HtmlBody { get; set; }
        public List<string?> ToRange { get; set; } = [];

    }
}
