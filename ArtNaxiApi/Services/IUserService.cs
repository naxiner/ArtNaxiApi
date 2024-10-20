using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;

namespace ArtNaxiApi.Services
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(RegistrDto model);
        Task<string> LoginUserAsync(LoginDto login);
        Guid GetCurrentUserId();
        Task<bool> UpdateUserByIdAsync(Guid id, UpdateUserDTO model);
    }
}