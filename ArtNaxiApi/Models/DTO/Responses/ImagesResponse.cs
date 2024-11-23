namespace ArtNaxiApi.Models.DTO.Responses
{
    public class ImagesResponse
    {
        public IEnumerable<ImageDto> Images { get; set; }
        public int TotalPages { get; set; } = 0;

        public ImagesResponse(IEnumerable<ImageDto> images, int totalPages) 
        {
            Images = images;
            TotalPages = totalPages;
        }
    }
}
