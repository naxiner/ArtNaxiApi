using ArtNaxiApi.Models;

namespace ArtNaxiApi.Repositories
{
    public interface IImageRepository
    {
        Task AddImageAsync(Image image);
        Task<IEnumerable<Image>> GetAllImagesAsync(int pageNumber, int pageSize);
        Task<Image?> GetImageByIdAsync(Guid id);
        Task<IEnumerable<Image>> GetImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> GetTotalImagesCountByUserIdAsync(Guid userId);
        Task<IEnumerable<Image>> GetRecentImagesAsync(int count);
        Task DeleteImageByIdAsync(Guid id);
    }
}