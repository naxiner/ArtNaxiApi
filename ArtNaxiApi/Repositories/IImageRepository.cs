using ArtNaxiApi.Models;

namespace ArtNaxiApi.Repositories
{
    public interface IImageRepository
    {
        Task AddImageAsync(Image image);
        Task<IEnumerable<Image>> GetAllImagesAsync(int pageNumber, int pageSize);
        Task<Image?> GetImageByIdAsync(Guid id);
        Task<IEnumerable<Image>> GetImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<IEnumerable<Image>> GetPublicImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> GetTotalImagesCountByUserIdAsync(Guid userId);
        Task<int> GetTotalPublicImagesCountByUserIdAsync(Guid userId);
        Task<IEnumerable<Image>> GetRecentImagesAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Image>> GetRecentPublicImagesAsync(int pageNumber, int pageSize);
        Task DeleteImageByIdAsync(Guid id);
        Task SetImageVisibilityAsync(Guid id, bool isPublic);
        Task SetAllUserImagesPrivateAsync(Guid userId);
    }
}