using ArtNaxiApi.Models;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtNaxiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ISDService _sdService;
        private readonly IUserService _userService;
        private readonly IImageRepository _imageRepository;

        public ImageController(
            ISDService sdService,
            IUserService userService,
            IImageRepository imageRepository)
        {
            _sdService = sdService;
            _userService = userService;
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
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userId = _userService.GetCurrentUserId();

            var imagePath = await _sdService.GenerateImageAsync(userId, sdRequest);

            return Ok(new { imagePath });
        }

        [Authorize]
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
