using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;

namespace ArtNaxiApi.Services
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(RegistrDto model);
        Task<string> LoginUserAsync(LoginDto login);
        Task<bool> UpdateUserByIdAsync(Guid id, UpdateUserDTO model);
        Task<bool> DeleteUserByIdAsync(Guid id);
        Guid GetCurrentUserId();
        Task<User> GetUserByIdAsync(Guid id);
    }
}