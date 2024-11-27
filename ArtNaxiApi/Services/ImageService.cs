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
        private readonly IUserService _userService;
        private readonly ISDService _sdService;

        public ImageService(
            IImageRepository imageRepository,
            ISDService sdService,
            IUserService userService)
        {
            _imageRepository = imageRepository;
            _sdService = sdService;
            _userService = userService;
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

            var imagesDto = images.Select(MapImageToDto);

            return (HttpStatusCode.OK, imagesDto, totalPages);
        }

        public async Task<(HttpStatusCode, ImageDto?)> GetImageByIdAsync(Guid id)
        {
            var image = await _imageRepository.GetImageByIdAsync(id);

            if (image == null)
            {
                return (HttpStatusCode.NotFound, null);     // Image not found
            }

            var imageDto = MapImageToDto(image);

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

            var imagesDto = userImages.Select(MapImageToDto);

            return (HttpStatusCode.OK, imagesDto, totalPages);
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetPublicImagesByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var userImages = await _imageRepository.GetPublicImagesByUserIdAsync(userId, pageNumber, pageSize);

            if (userImages == null)
            {
                return (HttpStatusCode.NotFound, Enumerable.Empty<ImageDto>(), 0);     // Images not found
            }

            var totalImagesCount = await _imageRepository.GetTotalPublicImagesCountByUserIdAsync(userId);
            var totalPages = (int)Math.Ceiling(totalImagesCount / (double)pageSize);

            var imagesDto = userImages.Select(MapImageToDto);

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

            var imagesDto = recentImages.Select(MapImageToDto);

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

            var imagesDto = recentPublicImages.Select(MapImageToDto);

            return (HttpStatusCode.OK, imagesDto, totalPages);
        }

        public async Task<(HttpStatusCode, IEnumerable<ImageDto>, int)> GetPopularPublicImagesAsync(int pageNumber, int pageSize)
        {
            var popularPublicImages = await _imageRepository.GetPopularPublicImagesAsync(pageNumber, pageSize);
            
            if (popularPublicImages == null)
            {
                return (HttpStatusCode.NotFound, Enumerable.Empty<ImageDto>(), 0);     // Images not found
            }

            var totalImagesCount = await _imageRepository.GetTotalPublicImagesCountAsync();
            var totalPages = (int)Math.Ceiling(totalImagesCount / (double)pageSize);

            var imagesDto = popularPublicImages.Select(MapImageToDto);

            return (HttpStatusCode.OK, imagesDto, totalPages);
        }

        public async Task<HttpStatusCode> MakeImagePublicAsync(Guid id)
        {
            var image = await _imageRepository.GetImageByIdAsync(id);
            if (image == null)
            {
                return HttpStatusCode.NotFound;
            }

            var currentUserId = _userService.GetCurrentUserId();
            if (image.UserId != currentUserId)
            {
                return HttpStatusCode.Forbidden;
            }

            await _imageRepository.SetImageVisibilityAsync(id, true);

            return HttpStatusCode.OK;
        }

        public async Task<HttpStatusCode> MakeImagePrivateAsync(Guid id)
        {
            var image = await _imageRepository.GetImageByIdAsync(id);
            if (image == null)
            {
                return HttpStatusCode.NotFound;
            }

            var currentUserId = _userService.GetCurrentUserId();
            if (image.UserId != currentUserId)
            {
                return HttpStatusCode.Forbidden;
            }

            await _imageRepository.SetImageVisibilityAsync(id, false);

            return HttpStatusCode.OK;
        }

        public async Task<(HttpStatusCode, ImageDto?)> GenerateImageAsync(SDRequest sdRequest)
        {
            return await _sdService.GenerateImageAsync(sdRequest);
        }

        public async Task<HttpStatusCode> DeleteImageByIdAsync(Guid id, ClaimsPrincipal userClaim)
        {
            return await _sdService.DeleteImageByIdAsync(id, userClaim);
        }

        private ImageDto MapImageToDto(Image image)
        {
            return new ImageDto
            {
                Id = image.Id,
                Url = image.Url,
                CreationTime = image.CreationTime,
                CreatedBy = image.CreatedBy,
                IsPublic = image.IsPublic,
                UserId = image.UserId,
                Request = new SDRequestDto
                {
                    Id = image.Request.Id,
                    Prompt = image.Request.Prompt,
                    NegativePrompt = image.Request.NegativePrompt,
                    Styles = image.Request.Styles,
                    Seed = image.Request.Seed,
                    SamplerName = image.Request.SamplerName,
                    Scheduler = image.Request.Scheduler,
                    Steps = image.Request.Steps,
                    CfgScale = image.Request.CfgScale,
                    Width = image.Request.Width,
                    Height = image.Request.Height
                }
            };
        }
    }
}
