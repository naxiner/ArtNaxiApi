namespace ArtNaxiApi.Models
{
    public class SDRequestStyle
    {
        public Guid SDRequestId { get; set; }
        public SDRequest SDRequest { get; set; }

        public Guid StyleId { get; set; }
        public Style Style { get; set; }
    }
}
