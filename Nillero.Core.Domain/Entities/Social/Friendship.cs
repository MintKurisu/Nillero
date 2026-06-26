namespace Nillero.Core.Domain.Entities.Social
{
    public class Friendship
    {
        public int Id { get; set; }
        public required string User1Id { get; set; }
        public required string User2Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
