using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;

namespace ArtNaxiApi.Services
{
    public interface IUserService
    {
        Task<User> RegisterAsync(RegistrDto model);
    }
}