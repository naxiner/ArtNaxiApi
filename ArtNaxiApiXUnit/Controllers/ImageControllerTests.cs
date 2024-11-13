using ArtNaxiApi.Controllers;
using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Dynamic;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApiXUnit.Controllers
{
    public class ImageControllerTests
    {
        private readonly Mock<ISDService> _sdServiceMock;
        private readonly Mock<IImageRepository> _imageRepositoryMock;
        private readonly ImageController _imageController;
        private readonly ClaimsPrincipal _user;

        public ImageControllerTests()
        {
            _sdServiceMock = new Mock<ISDService>();
            _imageRepositoryMock = new Mock<IImageRepository>();
            _imageController = new ImageController(_sdServiceMock.Object, _imageRepositoryMock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            _user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
            _imageController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };
        }

        [Fact]
        public async Task GetAllImagesAsync_ReturnsOk_WithImageList()
        {
            var images = new List<Image>
            {
                new Image { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, UserId = Guid.NewGuid(), Request = new SDRequest() },
                new Image { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, UserId = Guid.NewGuid(), Request = new SDRequest() },
                new Image { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, UserId = Guid.NewGuid(), Request = new SDRequest() }
            };

            int pageNumber = 1, pageSize = 10;
            _imageRepositoryMock.Setup(repo => repo.GetAllImagesAsync(pageNumber, pageSize)).ReturnsAsync(images);

            var result = await _imageController.GetAllImagesAsync();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Image>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedImages = Assert.IsAssignableFrom<IEnumerable<Image>>(okResult.Value);

            Assert.Equal(images.Count, returnedImages.Count());
        }

        [Fact]
        public async Task GetImageByIdAsync_ReturnsOk_WhenImageExists()
        {
            var imageId = Guid.NewGuid();
            var image = new Image
            {
                Id = imageId,
                Url = "/Images/example.png",
                CreationTime = DateTime.UtcNow,
                UserId = Guid.NewGuid(),
                Request = new SDRequest()
            };

            _imageRepositoryMock.Setup(repo => repo.GetImageByIdAsync(imageId)).ReturnsAsync(image);

            var result = await _imageController.GetImageByIdAsync(imageId);

            var actionResult = Assert.IsType<ActionResult<Image>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedImage = Assert.IsType<Image>(okResult.Value);

            Assert.Equal(imageId, returnedImage.Id);
        }

        [Fact]
        public async Task GetImageByIdAsync_ReturnsNotFound_WhenImageDoesNotExist()
        {
            var imageId = Guid.NewGuid();

            _imageRepositoryMock.Setup(repo => repo.GetImageByIdAsync(imageId)).ReturnsAsync((Image)null);

            var result = await _imageController.GetImageByIdAsync(imageId);

            var actionResult = Assert.IsType<ActionResult<Image>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task GenerateImage_ReturnsOk_WhenImageIsGeneratedSuccessfully()
        {
            var sdRequest = new SDRequest
            {
                Prompt = "an apple",
                NegativePrompt = "",
                Styles = null,
                SamplerName = "DPM++ SDE",
                Scheduler = "Karras",
                Steps = 7,
                CfgScale = 2,
                Width = 512,
                Height = 512
            };

            var expectedImage = new ImageDto
            {
                Id = Guid.NewGuid(),
                Url = "/Images/generated_image.png",
                CreationTime = DateTime.Now,
                Request = sdRequest
            };

            _sdServiceMock.Setup(service => service.GenerateImageAsync(sdRequest))
                .ReturnsAsync((HttpStatusCode.OK, expectedImage));

            var result = await _imageController.GenerateImage(sdRequest);

            var actionResult = Assert.IsType<ActionResult<Image>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

            var returnValue = Assert.IsType<Image>(okResult.Value);
            Assert.Equal(expectedImage.Id, returnValue.Id);
            Assert.Equal(expectedImage.Url, returnValue.Url);
            Assert.Equal(expectedImage.Request, returnValue.Request);
        }

        [Fact]
        public async Task GenerateImage_ReturnsInternalServerError_WhenErrorOccurs()
        {
            var sdRequest = new SDRequest
            {
                Prompt = "an apple",
                NegativePrompt = "",
                Styles = null,
                SamplerName = "DPM++ SDE",
                Scheduler = "Karras",
                Steps = 7,
                CfgScale = 2,
                Width = 512,
                Height = 512
            };

            _sdServiceMock.Setup(service => service.GenerateImageAsync(sdRequest))
                .ReturnsAsync((HttpStatusCode.InternalServerError, null));

            var result = await _imageController.GenerateImage(sdRequest);

            var actionResult = Assert.IsType<ActionResult<Image>>(result);
            Assert.IsType<ObjectResult>(actionResult.Result);
            var objectResult = actionResult.Result as ObjectResult;
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Error when saving image.", objectResult.Value);
        }

        [Fact]
        public async Task GenerateImage_ReturnsServiceUnavailable_WhenServiceIsUnavailable()
        {
            var sdRequest = new SDRequest
            {
                Prompt = "an apple",
                NegativePrompt = "",
                Styles = null,
                SamplerName = "DPM++ SDE",
                Scheduler = "Karras",
                Steps = 7,
                CfgScale = 2,
                Width = 512,
                Height = 512
            };

            _sdServiceMock.Setup(service => service.GenerateImageAsync(sdRequest))
                .ReturnsAsync((HttpStatusCode.ServiceUnavailable, null));

            var result = await _imageController.GenerateImage(sdRequest);

            var actionResult = Assert.IsType<ActionResult<Image>>(result);
            Assert.IsType<ObjectResult>(actionResult.Result);
            var objectResult = actionResult.Result as ObjectResult;
            Assert.Equal(503, objectResult.StatusCode);
            Assert.Equal("Stable Diffussion server Unavaliable.", objectResult.Value);
        }

        [Fact]
        public async Task DeleteImageById_ReturnsNotFound_WhenImageDoesNotExist()
        {
            var imageId = Guid.NewGuid();
            _sdServiceMock.Setup(service => service.DeleteImageByIdAsync(imageId, _user))
                .ReturnsAsync(HttpStatusCode.NotFound);

            var result = await _imageController.DeleteImageById(imageId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteImageById_ReturnsForbid_WhenUserIsForbidden()
        {
            var imageId = Guid.NewGuid();
            _sdServiceMock.Setup(service => service.DeleteImageByIdAsync(imageId, _user))
                .ReturnsAsync(HttpStatusCode.Forbidden);

            var result = await _imageController.DeleteImageById(imageId);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteImageById_ReturnsNoContent_WhenImageDeletedSuccessfully()
        {
            var imageId = Guid.NewGuid();
            _sdServiceMock.Setup(service => service.DeleteImageByIdAsync(imageId, _user))
                .ReturnsAsync(HttpStatusCode.NoContent);

            var result = await _imageController.DeleteImageById(imageId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteImageById_ReturnsBadRequest_WhenOtherErrorOccurs()
        {
            var imageId = Guid.NewGuid();
            _sdServiceMock.Setup(service => service.DeleteImageByIdAsync(imageId, _user))
                .ReturnsAsync(HttpStatusCode.BadRequest);

            var result = await _imageController.DeleteImageById(imageId);

            Assert.IsType<BadRequestResult>(result);
        }
    }
}
