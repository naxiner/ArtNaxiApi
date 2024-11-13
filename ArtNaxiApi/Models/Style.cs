namespace ArtNaxiApi.Models
{
    public class Style
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<SDRequestStyle> SDRequestStyles { get; set; } = new List<SDRequestStyle>();
    }
}
