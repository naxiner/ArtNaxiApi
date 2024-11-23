using ArtNaxiApi.Constants;
using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApi.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;

        public ImageService(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetAllImagesAsync(int pageNumber, int pageSize, ClaimsPrincipal userClaim)
        {
            if (!userClaim.IsInRole(Roles.Admin))
            {
                return (HttpStatusCode.Forbidden, Enumerable.Empty<ImageDto>(), 0);    // You are not allowed to get all images
            }

            var images = await _imageRepository.GetAllImagesAsync(pageNumber, pageSize);

            if (images == null)
            {
                return (HttpStatusCode.NotFound, Enumerable.Empty<ImageDto>(), 0);    // Images not found
            }

            var totalImagesCount = await _imageRepository.GetTotalImagesCountAsync();
            var totalPages = (int)Math.Ceiling(totalImagesCount / (double)pageSize);

            var imagesDto = images.Select(image => new ImageDto
            {
                Id = image.Id,
                Url = image.Url,
                CreationTime = image.CreationTime,
                CreatedBy = image.CreatedBy,
                Request = image.Request
            });

            return (HttpStatusCode.OK, imagesDto, totalPages);
        }

        public async Task<(HttpStatusCode, ImageDto?)> GetImageByIdAsync(Guid id)
        {
            var image = await _imageRepository.GetImageByIdAsync(id);

            if (image == null)
            {
                return (HttpStatusCode.NotFound, null);     // Image not found
            }

            var imageDto = new ImageDto
            {
                Id = image.Id,
                Url = image.Url,
                CreationTime = image.CreationTime,
                CreatedBy = image.CreatedBy,
                Request = image.Request
            };

            return (HttpStatusCode.OK, imageDto);
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize, ClaimsPrincipal userClaim)
        {
            if (!userClaim.IsInRole(Roles.Admin))
            {
                return (HttpStatusCode.Forbidden, Enumerable.Empty<ImageDto>(), 0);    // You are not allowed to get all user images
            }

            var userImages = await _imageRepository.GetImagesByUserIdAsync(userId, pageNumber, pageSize);

            if (userImages == null)
            {
                return (HttpStatusCode.NotFound, Enumerable.Empty<ImageDto>(), 0);     // Images not found
            }

            var totalImagesCount = await _imageRepository.GetTotalImagesCountByUserIdAsync(userId);
            var totalPages = (int)Math.Ceiling(totalImagesCount / (double)pageSize);

            var imagesDto = userImages.Select(image => new ImageDto
            {
                Id = image.Id,
                Url = image.Url,
                CreationTime = image.CreationTime,
                CreatedBy = image.CreatedBy,
                Request = image.Request
            });

            return (HttpStatusCode.OK, imagesDto, totalPages);
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetPublicImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var userImages = await _imageRepository.GetImagesByUserIdAsync(userId, pageNumber, pageSize);

            if (userImages == null)
            {
                return (HttpStatusCode.NotFound, Enumerable.Empty<ImageDto>(), 0);     // Images not found
            }

            var totalImagesCount = await _imageRepository.GetTotalPublicImagesCountByUserIdAsync(userId);
            var totalPages = (int)Math.Ceiling(totalImagesCount / (double)pageSize);

            var imagesDto = userImages.Select(image => new ImageDto
            {
                Id = image.Id,
                Url = image.Url,
                CreationTime = image.CreationTime,
                CreatedBy = image.CreatedBy,
                Request = image.Request
            });

            return (HttpStatusCode.OK, imagesDto, totalPages);
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetRecentImagesAsync(int pageNumber, int pageSize)
        {
            var recentImages = await _imageRepository.GetRecentImagesAsync(pageNumber, pageSize);

            if (recentImages == null)
            {
                return (HttpStatusCode.NotFound, Enumerable.Empty<ImageDto>(), 0);     // Images not found
            }

            var totalImagesCount = await _imageRepository.GetTotalImagesCountAsync();
            var totalPages = (int)Math.Ceiling(totalImagesCount / (double)pageSize);

            var imagesDto = recentImages.Select(image => new ImageDto
            {
                Id = image.Id,
                Url = image.Url,
                CreationTime = image.CreationTime,
                CreatedBy = image.CreatedBy,
                Request = image.Request
            });

            return (HttpStatusCode.OK, imagesDto, totalPages);
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetRecentPublicImagesAsync(int pageNumber, int pageSize)
        {
            var recentPublicImages = await _imageRepository.GetRecentPublicImagesAsync(pageNumber, pageSize);

            if (recentPublicImages == null)
            {
                return (HttpStatusCode.NotFound, Enumerable.Empty<ImageDto>(), 0);     // Images not found
            }

            var totalImagesCount = await _imageRepository.GetTotalPublicImagesCountAsync();
            var totalPages = (int)Math.Ceiling(totalImagesCount / (double)pageSize);

            var imagesDto = recentPublicImages.Select(image => new ImageDto
            {
                Id = image.Id,
                Url = image.Url,
                CreationTime = image.CreationTime,
                CreatedBy = image.CreatedBy,
                Request = image.Request
            });

            return (HttpStatusCode.OK, imagesDto, totalPages);
        }
    }
}
