using ArtNaxiApi.Models;

namespace ArtNaxiApi.Services
{
    public interface ISDService
    {
        Task<string> GenerateImageAsync(SDRequest request);
        Task<bool> DeleteImageByIdAsync(Guid id);
    }
}