using ArtNaxiApi.Models;

namespace ArtNaxiApi.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByNameAsync(string username);
        Task<bool> AddUserAsync(User user);
    }
}