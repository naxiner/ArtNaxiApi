namespace ArtNaxiApi.Models.DTO
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ProfilePictureUrl { get; set; }
        public List<ImageDto> Images { get; set; }
    }
}
