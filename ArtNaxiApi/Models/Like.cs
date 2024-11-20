namespace ArtNaxiApi.Models
{
    public class Like
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId{ get; set; }
        public User User { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

        public Guid EntityId { get; set; }
        public string EntityType { get; set; }
    }
}
