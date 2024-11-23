namespace ArtNaxiApi.Models.DTO.Responses
{
    public class AvatarResponse
    {
        public string Message { get; set; }
        public string AvatarUrl { get; set; }

        public AvatarResponse(string message, string avatarUrl)
        {
            Message = message;
            AvatarUrl = avatarUrl;
        }
    }
}
