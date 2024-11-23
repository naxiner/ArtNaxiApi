namespace ArtNaxiApi.Models.DTO.Responses
{
    public class LikeStatusResponse
    {
        public bool IsLiked { get; set; }

        public LikeStatusResponse(bool isLiked) 
        {
            IsLiked = isLiked;
        }
    }
}
