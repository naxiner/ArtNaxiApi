﻿using ArtNaxiApi.Filters;
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

            return Ok(new { pageOfImages });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImageByIdAsync(Guid id)
        {
            var imageById = await _imageRepository.GetImageByIdAsync(id);

            if (imageById == null)
            {
                return NotFound();
            }

            return Ok(new { imageById });
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

            return Ok(new { recentImages });
        }

        [HttpGet("recent/public")]
        public async Task<ActionResult<IEnumerable<Image>>> GetRecentPublicImagesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var recentImages = await _imageRepository.GetRecentPublicImagesAsync(pageNumber, pageSize);

            return Ok(new { recentImages });
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
                HttpStatusCode.OK => Ok(image),
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
