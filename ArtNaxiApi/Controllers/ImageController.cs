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
        public async Task<ActionResult<IEnumerable<Image>>> GetAllImagesAsync()
        {
            var allImages = await _imageRepository.GetAllImagesAsync();
            return Ok(allImages);
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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Image>> GenerateImage(SDRequest sdRequest)
        {
            var (result, imagePath) = await _sdService.GenerateImageAsync(sdRequest);
            return result switch
            {
                HttpStatusCode.OK => Ok(new { imagePath }),
                HttpStatusCode.InternalServerError => StatusCode(500, "Error when saving image."),
                HttpStatusCode.ServiceUnavailable => StatusCode(503, "Stable Diffussion server Unavaliable."),
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
