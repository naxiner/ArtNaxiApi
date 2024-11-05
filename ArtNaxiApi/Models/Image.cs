namespace ArtNaxiApi.Models
{
    public class Image
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public bool IsPublic { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public SDRequest Request { get; set; }
    }
}
