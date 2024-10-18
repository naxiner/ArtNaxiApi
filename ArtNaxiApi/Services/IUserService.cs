using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;

namespace ArtNaxiApi.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> RegisterUserAsync(RegistrDto model);
        Task<string> LoginUserAsync(LoginDto login);
    }
}