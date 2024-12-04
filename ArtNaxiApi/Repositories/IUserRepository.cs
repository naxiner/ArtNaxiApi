using ArtNaxiApi.Models;

namespace ArtNaxiApi.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<IQueryable<User>> GetUsersByQueryAsync(string query);
        Task<int> GetTotalCountUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByNameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> GetUserByNameOrEmailAsync(string usernameOrEmail);
        Task<bool> AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
    }
}