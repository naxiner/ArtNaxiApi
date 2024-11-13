namespace ArtNaxiApi.Models.DTO
{
    public class ImageDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public DateTime CreationTime { get; set; }
        public string CreatedBy { get; set; }
        public SDRequest Request { get; set; }
    }
}
