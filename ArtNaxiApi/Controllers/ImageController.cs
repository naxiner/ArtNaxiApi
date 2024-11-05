using ArtNaxiApi.Models;
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

        public ImageController(
            ISDService sdService,
            IImageRepository imageRepository)
        {
            _sdService = sdService;
            _imageRepository = imageRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Image>>> GetAllImagesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var pageOfImages = await _imageRepository.GetAllImagesAsync(pageNumber, pageSize);
            
            return Ok(pageOfImages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImageByIdAsync(Guid id)
        {
            var imageById = await _imageRepository.GetImageByIdAsync(id);

            if (imageById == null)
            {
                return NotFound();
            }

            return Ok(imageById);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<(IEnumerable<Image>, int)>> GetImagesByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            var userImages = await _imageRepository.GetImagesByUserIdAsync(userId, pageNumber, pageSize);

            if (userImages == null)
            {
                return NotFound();
            }

            var totalImagesCount = await _imageRepository.GetTotalImagesCountByUserIdAsync(userId);
            var totalPages = (int)Math.Ceiling(totalImagesCount / (double)pageSize);

            return Ok(new { userImages, totalPages });
        }

        [HttpGet("user/{userId}/public")]
        public async Task<ActionResult<(IEnumerable<Image>, int)>> GetPublicImagesByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            var userImages = await _imageRepository.GetPublicImagesByUserIdAsync(userId, pageNumber, pageSize);

            if (userImages == null)
            {
                return NotFound();
            }

            var totalImagesCount = await _imageRepository.GetTotalPublicImagesCountByUserIdAsync(userId);
            var totalPages = (int)Math.Ceiling(totalImagesCount / (double)pageSize);

            return Ok(new { userImages, totalPages });
        }

        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<Image>>> GetRecentImagesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var recentImages = await _imageRepository.GetRecentImagesAsync(pageNumber, pageSize);
            
            return Ok(recentImages);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Image>> GenerateImage(SDRequest sdRequest)
        {
            var (result, image) = await _sdService.GenerateImageAsync(sdRequest);
            
            return result switch
            {
                HttpStatusCode.OK => Ok(image),
                HttpStatusCode.InternalServerError => StatusCode(500, "Error when saving image."),
                HttpStatusCode.ServiceUnavailable => StatusCode(503, "Stable Diffussion server Unavaliable."),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpPut("{id}/make-public")]
        public async Task<ActionResult> MakeImagePublic(Guid id)
        {
            var result = await _sdService.MakeImagePublicAsync(id);

            return result switch
            {
                HttpStatusCode.OK => Ok("Image visibility changed to public succesful."),
                HttpStatusCode.NotFound => NotFound("Image not found."),
                HttpStatusCode.Forbidden => Forbid(),
                _ => BadRequest()
            };
        }

        [Authorize]
        [HttpPut("{id}/make-private")]
        public async Task<ActionResult> MakeImagePrivate(Guid id)
        {
            var result = await _sdService.MakeImagePrivateAsync(id);

            return result switch
            {
                HttpStatusCode.OK => Ok("Image visibility changed to private succesful."),
                HttpStatusCode.NotFound => NotFound("Image not found."),
                HttpStatusCode.Forbidden => Forbid(),
                _ => BadRequest()
            };
        }

        [Authorize]
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
