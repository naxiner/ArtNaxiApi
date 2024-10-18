using ArtNaxiApi.Models;

namespace ArtNaxiApi.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}