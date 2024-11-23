namespace ArtNaxiApi.Models.DTO.Responses
{
    public class ImageResponse
    {
        public ImageDto Image { get; set; }

        public ImageResponse(ImageDto image)
        {
            Image = image;
        }
    }
}
