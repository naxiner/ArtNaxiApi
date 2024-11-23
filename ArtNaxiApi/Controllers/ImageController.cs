using ArtNaxiApi.Filters;
using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Models.DTO.Responses;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ArtNaxiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ISDService _sdService;
        private readonly IImageRepository _imageRepository;
        private readonly IImageService _imageService;

        public ImageController(
            ISDService sdService,
            IImageRepository imageRepository,
            IImageService imageService)
        {
            _sdService = sdService;
            _imageRepository = imageRepository;
            _imageService = imageService;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetAllImagesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var (result, allImages, totalPages) = await _imageService.GetAllImagesAsync(pageNumber, pageSize, User);

            return result switch
            {
                HttpStatusCode.OK => Ok(new ImagesResponse(allImages, totalPages)),
                HttpStatusCode.Forbidden => Forbid(),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Images not found.")),
                _ => BadRequest()
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetImageByIdAsync(Guid id)
        {
            var (result, imageById) = await _imageService.GetImageByIdAsync(id);

            return result switch
            {
                HttpStatusCode.OK => Ok(new ImageResponse(imageById)),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Image not found.")),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult> GetImagesByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            var (result, allImages, totalPages) = await _imageService.GetImagesByUserIdAsync(userId, pageNumber, pageSize, User);

            return result switch
            {
                HttpStatusCode.OK => Ok(new ImagesResponse(allImages, totalPages)),
                HttpStatusCode.Forbidden => Forbid(),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Images not found.")),
                _ => BadRequest()
            };
        }

        [HttpGet("user/{userId}/public")]
        public async Task<ActionResult> GetPublicImagesByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            var (result, userPublicImages, totalPages) = await _imageService.GetPublicImagesByUserIdAsync(userId, pageNumber, pageSize);

            return result switch
            {
                HttpStatusCode.OK => Ok(new ImagesResponse(userPublicImages, totalPages)),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Images not found.")),
                _ => BadRequest()
            };
        }

        [HttpGet("recent")]
        public async Task<ActionResult> GetRecentImagesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var (result, recentImages, totalPages) = await _imageService.GetRecentImagesAsync(pageNumber, pageSize);

            return result switch
            {
                HttpStatusCode.OK => Ok(new ImagesResponse(recentImages, totalPages)),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Images not found.")),
                _ => BadRequest()
            };
        }

        [HttpGet("recent/public")]
        public async Task<ActionResult> GetRecentPublicImagesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var (result, recentPublicImages, totalPages) = await _imageService.GetRecentPublicImagesAsync(pageNumber, pageSize);

            return result switch
            {
                HttpStatusCode.OK => Ok(new ImagesResponse(recentPublicImages, totalPages)),
                HttpStatusCode.NotFound => NotFound(new MessageResponse("Images not found.")),
                _ => BadRequest()
            };
        }
        
        [HttpGet("popular/public")]
        public async Task<ActionResult<IEnumerable<Image>>> GetPopularPublicImagesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var popularImages = await _imageRepository.GetPopularPublicImagesAsync(pageNumber, pageSize);

            return Ok(new { popularImages });
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpPost]
        public async Task<ActionResult> GenerateImage(SDRequest sdRequest)
        {
            var (result, image) = await _sdService.GenerateImageAsync(sdRequest);

            return result switch
            {
                HttpStatusCode.OK => Ok(new ImageResponse(image)),
                HttpStatusCode.InternalServerError => StatusCode(500, new { message = "Error when saving image." }),
                HttpStatusCode.ServiceUnavailable => StatusCode(503, new { message = "Stable Diffussion server Unavaliable." }),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpPut("{id}/make-public")]
        public async Task<ActionResult> MakeImagePublic(Guid id)
        {
            var result = await _sdService.MakeImagePublicAsync(id);

            return result switch
            {
                HttpStatusCode.OK => Ok(),
                HttpStatusCode.NotFound => NotFound(new { message = "Image not found." }),
                HttpStatusCode.Forbidden => Forbid(),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpPut("{id}/make-private")]
        public async Task<ActionResult> MakeImagePrivate(Guid id)
        {
            var result = await _sdService.MakeImagePrivateAsync(id);

            return result switch
            {
                HttpStatusCode.OK => Ok(),
                HttpStatusCode.NotFound => NotFound(new { message = "Image not found." }),
                HttpStatusCode.Forbidden => Forbid(),
                _ => BadRequest()
            };
        }

        [Authorize]
        [ServiceFilter(typeof(CheckBanAttribute))]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteImageById(Guid id)
        {
            var result = await _sdService.DeleteImageByIdAsync(id, User);

            return result switch
            {
                HttpStatusCode.NotFound => NotFound(),
                HttpStatusCode.Forbidden => Forbid(),
                HttpStatusCode.NoContent => NoContent(),
                _ => BadRequest()
            };
        }
    }
}
