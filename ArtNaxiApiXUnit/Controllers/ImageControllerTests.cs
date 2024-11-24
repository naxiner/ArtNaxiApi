using ArtNaxiApi.Controllers;
using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Models.DTO.Responses;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly ImageController _imageController;
        private readonly ClaimsPrincipal _user;

        public ImageControllerTests()
        {
            _sdServiceMock = new Mock<ISDService>();
            _imageServiceMock = new Mock<IImageService>();
            _imageController = new ImageController(_sdServiceMock.Object, _imageServiceMock.Object);

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
            // Arrange
            var images = new List<ImageDto>
            {
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() }
            };

            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(3 / (double)pageSize);
            
            _imageServiceMock.Setup(service => service.GetAllImagesAsync(pageNumber, pageSize, _user))
                .ReturnsAsync((HttpStatusCode.OK, images, totalPages));
            
            // Act
            var result = await _imageController.GetAllImagesAsync();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ImagesResponse>(objectResult.Value);
            Assert.Equal(images, response.Images);
            Assert.Equal(totalPages, response.TotalPages);
        }

        [Fact]
        public async Task GetAllImagesAsync_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var images = new List<ImageDto>();
            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(0 / (double)pageSize);
            
            _imageServiceMock.Setup(service => service.GetAllImagesAsync(pageNumber, pageSize, _user))
                .ReturnsAsync((HttpStatusCode.Forbidden, images, totalPages));

            // Act
            var result = await _imageController.GetAllImagesAsync();

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetAllImagesAsync_ReturnsNotFound_WhenImagesDoesNotExist()
        {
            // Arrange
            var images = new List<ImageDto>();

            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _imageServiceMock.Setup(service => service.GetAllImagesAsync(pageNumber, pageSize, _user))
                .ReturnsAsync((HttpStatusCode.NotFound, images, totalPages));

            // Act
            var result = await _imageController.GetAllImagesAsync();

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Images not found.", response.Message);
        }

        [Fact]
        public async Task GetImageByIdAsync_ReturnsOk_WhenImageExists()
        {
            // Arrange
            var image = new ImageDto
            {
                Id = Guid.NewGuid(),
                Url = "/Images/example.png",
                CreationTime = DateTime.UtcNow,
                CreatedBy = "CreatedBy",
                IsPublic = false,
                Request = new SDRequest()
            };

            _imageServiceMock.Setup(service => service.GetImageByIdAsync(image.Id))
                .ReturnsAsync((HttpStatusCode.OK, image));

            // Act
            var result = await _imageController.GetImageByIdAsync(image.Id);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ImageResponse>(objectResult.Value);
            Assert.Equal(image, response.Image);
        }

        [Fact]
        public async Task GetImageByIdAsync_ReturnsNotFound_WhenImageDoesNotExist()
        {
            // Arrange
            var image = new ImageDto
            {
                Id = Guid.NewGuid(),
                Url = "/Images/example.png",
                CreationTime = DateTime.UtcNow,
                CreatedBy = "CreatedBy",
                IsPublic = false,
                Request = new SDRequest()
            };

            _imageServiceMock.Setup(service => service.GetImageByIdAsync(image.Id))
                .ReturnsAsync((HttpStatusCode.NotFound, image));

            // Act
            var result = await _imageController.GetImageByIdAsync(image.Id);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Image not found.", response.Message);
        }

        [Fact]
        public async Task GetImagesByUserIdAsync_ReturnsOk_WithAllUserImages()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var images = new List<ImageDto>
            {
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = true, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() }
            };

            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(3 / (double)pageSize);

            _imageServiceMock.Setup(service => service
                .GetImagesByUserIdAsync(userId, pageNumber, pageSize, _user))
                .ReturnsAsync((HttpStatusCode.OK, images, totalPages));

            // Act
            var result = await _imageController.GetImagesByUserIdAsync(userId, pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ImagesResponse>(objectResult.Value);
            Assert.Equal(images, response.Images);
            Assert.Equal(totalPages, response.TotalPages);
        }

        [Fact]
        public async Task GetImagesByUserIdAsync_Forbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var images = new List<ImageDto>();
            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _imageServiceMock.Setup(service => service
                .GetImagesByUserIdAsync(userId, pageNumber, pageSize, _user))
                .ReturnsAsync((HttpStatusCode.Forbidden, images, totalPages));

            // Act
            var result = await _imageController.GetImagesByUserIdAsync(userId, pageNumber, pageSize);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetImagesByUserIdAsync_ReturnsNotFound_WhenImagesDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var images = new List<ImageDto>();

            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _imageServiceMock.Setup(service => service
                .GetImagesByUserIdAsync(userId, pageNumber, pageSize, _user))
                .ReturnsAsync((HttpStatusCode.NotFound, images, totalPages));

            // Act
            var result = await _imageController.GetImagesByUserIdAsync(userId, pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Images not found.", response.Message);
        }

        [Fact]
        public async Task GetPublicImagesByUserIdAsync_ReturnsOk_WithUserPublicImagesList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var images = new List<ImageDto>
            {
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = true, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() }
            };

            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(1 / (double)pageSize);

            _imageServiceMock.Setup(service => service
                .GetPublicImagesByUserIdAsync(userId, pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.OK, images, totalPages));

            // Act
            var result = await _imageController.GetPublicImagesByUserIdAsync(userId, pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ImagesResponse>(objectResult.Value);
            Assert.Equal(images, response.Images);
            Assert.Equal(totalPages, response.TotalPages);
        }

        [Fact]
        public async Task GetPublicImagesByUserIdAsync_ReturnsNotFound_WhenImagesDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var images = new List<ImageDto>();

            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _imageServiceMock.Setup(service => service
                .GetPublicImagesByUserIdAsync(userId, pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.NotFound, images, totalPages));

            // Act
            var result = await _imageController.GetPublicImagesByUserIdAsync(userId, pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Images not found.", response.Message);
        }

        [Fact]
        public async Task GetRecentImagesAsync_ReturnsOk_WithRecentImagesList()
        {
            // Arrange
            var images = new List<ImageDto>
            {
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = true, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() }
            };

            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(3 / (double)pageSize);

            _imageServiceMock.Setup(service => service
                .GetRecentImagesAsync(pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.OK, images, totalPages));

            // Act
            var result = await _imageController.GetRecentImagesAsync(pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ImagesResponse>(objectResult.Value);
            Assert.Equal(images, response.Images);
            Assert.Equal(totalPages, response.TotalPages);
        }

        [Fact]
        public async Task GetRecentImagesAsync_ReturnsNotFound_WhenImagesDoesNotExist()
        {
            // Arrange
            var images = new List<ImageDto>();
            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _imageServiceMock.Setup(service => service
                .GetRecentImagesAsync(pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.NotFound, images, totalPages));

            // Act
            var result = await _imageController.GetRecentImagesAsync(pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Images not found.", response.Message);
        }

        [Fact]
        public async Task GetRecentPublicImagesAsync_ReturnsOk_WithRecentImagesList()
        {
            // Arrange
            var images = new List<ImageDto>
            {
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = true, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() }
            };

            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(1 / (double)pageSize);

            _imageServiceMock.Setup(service => service
                .GetRecentPublicImagesAsync(pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.OK, images, totalPages));

            // Act
            var result = await _imageController.GetRecentPublicImagesAsync(pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ImagesResponse>(objectResult.Value);
            Assert.Equal(images, response.Images);
            Assert.Equal(totalPages, response.TotalPages);
        }

        [Fact]
        public async Task GetRecentPublicImagesAsync_ReturnsNotFound_WhenImagesDoesNotExist()
        {
            // Arrange
            var images = new List<ImageDto>();
            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _imageServiceMock.Setup(service => service
                .GetRecentPublicImagesAsync(pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.NotFound, images, totalPages));

            // Act
            var result = await _imageController.GetRecentPublicImagesAsync(pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Images not found.", response.Message);
        }

        [Fact]
        public async Task GetPopularPublicImagesAsync_ReturnsOk_WithRecentImagesList()
        {
            // Arrange
            var images = new List<ImageDto>
            {
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = true, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() },
                new ImageDto { Id = Guid.NewGuid(), Url = $"/Images/{Guid.NewGuid}.png", CreationTime = DateTime.UtcNow, CreatedBy = "CreatedBy", IsPublic = false, Request = new SDRequest() }
            };

            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(1 / (double)pageSize);

            _imageServiceMock.Setup(service => service
                .GetPopularPublicImagesAsync(pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.OK, images, totalPages));

            // Act
            var result = await _imageController.GetPopularPublicImagesAsync(pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ImagesResponse>(objectResult.Value);
            Assert.Equal(images, response.Images);
            Assert.Equal(totalPages, response.TotalPages);
        }

        [Fact]
        public async Task GetPopularPublicImagesAsync_ReturnsNotFound_WhenImagesDoesNotExist()
        {
            // Arrange
            var images = new List<ImageDto>();
            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _imageServiceMock.Setup(service => service
                .GetPopularPublicImagesAsync(pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.NotFound, images, totalPages));

            // Act
            var result = await _imageController.GetPopularPublicImagesAsync(pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Images not found.", response.Message);
        }

        [Fact]
        public async Task GenerateImage_ReturnsOk_WhenImageIsGeneratedSuccessfully()
        {
            // Arrange
            var sdRequest = new SDRequest
            {
                Prompt = "an apple",
                NegativePrompt = "",
                Styles = [],
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
                CreatedBy = "CreatedBy",
                IsPublic = false,
                Request = sdRequest
            };

            _sdServiceMock.Setup(service => service.GenerateImageAsync(sdRequest))
                .ReturnsAsync((HttpStatusCode.OK, expectedImage));

            // Act
            var result = await _imageController.GenerateImage(sdRequest);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ImageResponse>(objectResult.Value);
            Assert.Equal(expectedImage, response.Image);
        }

        [Fact]
        public async Task GenerateImage_ReturnsInternalServerError_WhenErrorOccurs()
        {
            // Arrange
            var sdRequest = new SDRequest
            {
                Prompt = "an apple",
                NegativePrompt = "",
                Styles = [],
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
                CreatedBy = "CreatedBy",
                IsPublic = false,
                Request = sdRequest
            };

            _sdServiceMock.Setup(service => service.GenerateImageAsync(sdRequest))
                .ReturnsAsync((HttpStatusCode.InternalServerError, expectedImage));

            // Act
            var result = await _imageController.GenerateImage(sdRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Error when saving image.", response.Message);
        }

        [Fact]
        public async Task GenerateImage_ReturnsServiceUnavailable_WhenServiceIsUnavailable()
        {
            // Arrange
            var sdRequest = new SDRequest
            {
                Prompt = "an apple",
                NegativePrompt = "",
                Styles = [],
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
                CreatedBy = "CreatedBy",
                IsPublic = false,
                Request = sdRequest
            };

            _sdServiceMock.Setup(service => service.GenerateImageAsync(sdRequest))
                .ReturnsAsync((HttpStatusCode.ServiceUnavailable, expectedImage));

            // Act
            var result = await _imageController.GenerateImage(sdRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Stable Diffussion server Unavaliable.", response.Message);
        }

        [Fact]
        public async Task MakeImagePublic_ReturnsOk_WhenMakePublicSuccesfully()
        {
            // Arrange
            var imageId = Guid.NewGuid();

            _imageServiceMock.Setup(service => service.MakeImagePublicAsync(imageId))
                .ReturnsAsync(HttpStatusCode.OK);

            // Act
            var result = await _imageController.MakeImagePublic(imageId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task MakeImagePublic_ReturnsNotFound_WhenImageDoesNotExist()
        {
            // Arrange
            var imageId = Guid.NewGuid();

            _imageServiceMock.Setup(service => service.MakeImagePublicAsync(imageId))
                .ReturnsAsync(HttpStatusCode.NotFound);

            // Act
            var result = await _imageController.MakeImagePublic(imageId);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Image not found.", response.Message);
        }

        [Fact]
        public async Task MakeImagePublic_ReturnsForbidden_WhenUserNotOwner()
        {
            // Arrange
            var imageId = Guid.NewGuid();

            _imageServiceMock.Setup(service => service.MakeImagePublicAsync(imageId))
                .ReturnsAsync(HttpStatusCode.Forbidden);

            // Act
            var result = await _imageController.MakeImagePublic(imageId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task MakeImagePrivate_ReturnsOk_WhenMakePrivateSuccesfully()
        {
            // Arrange
            var imageId = Guid.NewGuid();

            _imageServiceMock.Setup(service => service.MakeImagePrivateAsync(imageId))
                .ReturnsAsync(HttpStatusCode.OK);

            // Act
            var result = await _imageController.MakeImagePrivate(imageId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task MakeImagePrivate_ReturnsNotFound_WhenImageDoesNotExist()
        {
            // Arrange
            var imageId = Guid.NewGuid();

            _imageServiceMock.Setup(service => service.MakeImagePrivateAsync(imageId))
                .ReturnsAsync(HttpStatusCode.NotFound);

            // Act
            var result = await _imageController.MakeImagePrivate(imageId);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Image not found.", response.Message);
        }

        [Fact]
        public async Task MakeImagePrivate_ReturnsForbidden_WhenUserNotOwner()
        {
            // Arrange
            var imageId = Guid.NewGuid();

            _imageServiceMock.Setup(service => service.MakeImagePrivateAsync(imageId))
                .ReturnsAsync(HttpStatusCode.Forbidden);

            // Act
            var result = await _imageController.MakeImagePrivate(imageId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteImageById_ReturnsNotFound_WhenImageDoesNotExist()
        {
            // Arrange
            var imageId = Guid.NewGuid();

            _sdServiceMock.Setup(service => service.DeleteImageByIdAsync(imageId, _user))
                .ReturnsAsync(HttpStatusCode.NotFound);

            // Act
            var result = await _imageController.DeleteImageById(imageId);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Image not found.", response.Message);
        }

        [Fact]
        public async Task DeleteImageById_ReturnsForbid_WhenUserIsForbidden()
        {
            // Arrange
            var imageId = Guid.NewGuid();

            _sdServiceMock.Setup(service => service.DeleteImageByIdAsync(imageId, _user))
                .ReturnsAsync(HttpStatusCode.Forbidden);

            // Act
            var result = await _imageController.DeleteImageById(imageId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteImageById_ReturnsNoContent_WhenImageDeletedSuccessfully()
        {
            // Arrange
            var imageId = Guid.NewGuid();

            _sdServiceMock.Setup(service => service.DeleteImageByIdAsync(imageId, _user))
                .ReturnsAsync(HttpStatusCode.NoContent);

            // Act
            var result = await _imageController.DeleteImageById(imageId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
