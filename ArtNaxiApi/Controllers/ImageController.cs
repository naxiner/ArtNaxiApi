using ArtNaxiApi.Models;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArtNaxiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ISDService _sdService;
        private readonly IImageRepository _imageRepository;

        public ImageController(ISDService sdService, IImageRepository imageRepository)
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

        [HttpPost]
        public async Task<ActionResult<Image>> GenerateImage(SDRequest sdRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var imagePath = await _sdService.GenerateImageAsync(sdRequest);

            return Ok(imagePath);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteImageById(Guid id)
        {
            var result = await _sdService.DeleteImageByIdAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
