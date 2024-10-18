﻿using ArtNaxiApi.Data;
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

        public async Task<IEnumerable<Image>> GetAllImagesAsync()
        {
            return await _context.Images
                .Include(i => i.Request)
                .ToListAsync();
        }

        public async Task<Image?> GetImageByIdAsync(Guid id)
        {
            var imageById = await _context.Images
                .Include(i => i.Request)
                .FirstOrDefaultAsync(i => i.Id == id);

            return imageById;
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
    }
}