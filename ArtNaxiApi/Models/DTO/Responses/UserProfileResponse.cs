namespace ArtNaxiApi.Models.DTO.Responses
{
    public class UserProfileResponse
    {
        public UserProfileDto UserProfileDto { get; set; }

        public UserProfileResponse(UserProfileDto userPorfileDto)
        {
            UserProfileDto = userPorfileDto;
        }
    }
}
