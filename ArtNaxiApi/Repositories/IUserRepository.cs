using ArtNaxiApi.Models;

namespace ArtNaxiApi.Repositories
{
    public interface IUserRepository
    {
        Task<bool> AddUserAsync(User user);
    }
}