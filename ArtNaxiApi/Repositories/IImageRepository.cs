﻿using ArtNaxiApi.Models;

namespace ArtNaxiApi.Repositories
{
    public interface IImageRepository
    {
        Task AddImageAsync(Image image);
        Task<IEnumerable<Image>> GetAllImagesAsync();
        Task<Image?> GetImageByIdAsync(Guid id);
        Task DeleteImageByIdAsync(Guid id);
    }
}