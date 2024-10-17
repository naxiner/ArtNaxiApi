using ArtNaxiApi.Data;
using ArtNaxiApi.Models;

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
            _context.Images.Add(image);
            await _context.SaveChangesAsync();
        }
    }
}
