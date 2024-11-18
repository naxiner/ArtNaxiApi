using ArtNaxiApi.Data;
using ArtNaxiApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtNaxiApi.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly AppDbContext _context;
        public ImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddImageAsync(Image image)
        {
            await _context.Images.AddAsync(image);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Image>> GetAllImagesAsync(int pageNumber, int pageSize)
        {
            return await _context.Images
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(i => i.Request)
                .ToListAsync();
        }

        public async Task<Image?> GetImageByIdAsync(Guid id)
        {
            return await _context.Images
                .Include(i => i.Request)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Image>> GetImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.Images
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(i => i.Request)
                .ToListAsync();
        }

        public async Task<IEnumerable<Image>> GetPublicImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.Images
                .Where(i => i.UserId == userId && i.IsPublic)
                .OrderByDescending(i => i.CreationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(i => i.Request)
                .ToListAsync();
        }

        public async Task<int> GetTotalImagesCountByUserIdAsync(Guid userId)
        {
            return await _context.Images
                .Where(image => image.UserId == userId)
                .CountAsync();
        }

        public async Task<int> GetTotalPublicImagesCountByUserIdAsync(Guid userId)
        {
            return await _context.Images
                .Where(image => image.UserId == userId && image.IsPublic)
                .CountAsync();
        }

        public async Task<IEnumerable<Image>> GetRecentImagesAsync(int pageNumber, int pageSize)
        {
            return await _context.Images
                .OrderByDescending(i => i.CreationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(i => i.Request)
                .ToListAsync();
        }

        public async Task<IEnumerable<Image>> GetRecentPublicImagesAsync(int pageNumber, int pageSize)
        {
            return await _context.Images
                .Where(i => i.IsPublic)
                .OrderByDescending(i => i.CreationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(i => i.Request)
                .ToListAsync();
        }

        public async Task DeleteImageByIdAsync(Guid id)
        {
            var imageById = await _context.Images.FindAsync(id);

            if (imageById == null)
            {
                throw new KeyNotFoundException();
            }

            _context.Images.Remove(imageById);
            await _context.SaveChangesAsync();
        }

        public async Task SetImageVisibilityAsync(Guid id, bool isPublic)
        {
            var imageById = await _context.Images.FindAsync(id);

            if (imageById == null)
            {
                throw new KeyNotFoundException();
            }

            imageById.IsPublic = isPublic;

            _context.Images.Update(imageById);
            await _context.SaveChangesAsync();
        }

        public async Task SetAllUserImagesPrivateAsync(Guid userId)
        {
            var userImages = await _context.Images
                .Where(i => i.UserId == userId).ToListAsync();

            if (userImages.Any())
            {
                foreach (var image in userImages)
                {
                    image.IsPublic = false;
                    _context.Images.Update(image);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
