using ArtNaxiApi.Models;

namespace ArtNaxiApi.Repositories
{
    public interface IImageRepository
    {
        Task AddImageAsync(Image image);
    }
}