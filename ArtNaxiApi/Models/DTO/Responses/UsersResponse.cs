namespace ArtNaxiApi.Models.DTO.Responses
{
    public class UsersResponse
    {
        public IEnumerable<UserDto> Users { get; set; }
        public int TotalPages { get; set; } = 0;

        public UsersResponse(IEnumerable<UserDto> users, int totalPages)
        {
            Users = users;
            TotalPages = totalPages;
        }
    }
}
