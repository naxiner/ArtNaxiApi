using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Moq;
using System.Net;
using ArtNaxiApi.Models;
using Moq.Protected;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Security.Claims;
using ArtNaxiApi.Constants;

namespace ArtNaxiApiXUnit.Services
{
    public class SDServiceTests
    {
        private readonly Mock<IImageRepository> _imageRepositoryMock;
        private readonly Mock<IStyleRepository> _styleRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;
        private readonly Mock<ILikeRepository> _likeRepositoryMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;

        private readonly HttpClient _mockHttpClient;
        private readonly SDService _sdService;

        public SDServiceTests()
        {
            _imageRepositoryMock = new Mock<IImageRepository>();
            _styleRepositoryMock = new Mock<IStyleRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
            _likeRepositoryMock = new Mock<ILikeRepository>();
            _userServiceMock = new Mock<IUserService>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["StableDiffusion:ApiUrlTextToImg"]).Returns("http://fakeapi.com");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

            var fakeResponse = new
            {
                images = new[] { Convert.ToBase64String(Encoding.UTF8.GetBytes("FakeImage")) }
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(fakeResponse), Encoding.UTF8, "application/json")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _sdService = new SDService(
                _mockHttpClient,
                _imageRepositoryMock.Object,
                _styleRepositoryMock.Object,
                _userRepositoryMock.Object,
                _userProfileRepositoryMock.Object,
                _likeRepositoryMock.Object,
                _userServiceMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public async Task GenerateImageAsync_ReturnsOK_WithImageDto()
        {
            // Arrange
            var request = new SDRequest
            {
                Id = Guid.NewGuid(),
                Prompt = "Test prompt",
                NegativePrompt = "Test negative prompt",
                Styles = new List<string> { "Style1" },
                Seed = 333333,
                SamplerName = "DPM++ SDE",
                Scheduler = "Karras",
                Steps = 7,
                CfgScale = 2,
                Width = 512,
                Height = 512
            };

            _userServiceMock.Setup(u => u.GetCurrentUserId()).Returns(Guid.NewGuid());
            _userRepositoryMock.Setup(u => u.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new User { Username = "TestUser" });
            _userProfileRepositoryMock.Setup(u => u.GetProfileByUserIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new UserProfile { Images = new List<Image>() });

            _styleRepositoryMock.Setup(s => s.GetStyleByNameAsync("Style1"))
                .ReturnsAsync(new Style { Id = Guid.NewGuid(), Name = "Style1" });

            _imageRepositoryMock.Setup(i => i.AddImageAsync(It.IsAny<Image>()))
                .Returns(Task.CompletedTask);
            _userProfileRepositoryMock.Setup(u => u.UpdateAsync(It.IsAny<UserProfile>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sdService.GenerateImageAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.NotNull(result.Item2);
            Assert.Equal("TestUser", result.Item2?.CreatedBy);

            _imageRepositoryMock.Verify(i => i.AddImageAsync(It.IsAny<Image>()), Times.Once);
            _userProfileRepositoryMock.Verify(u => u.UpdateAsync(It.IsAny<UserProfile>()), Times.Once);
        }

        [Fact]
        public async Task GenerateImageAsync_ReturnsServiceUnavailable_WhenApiResponseFailed()
        {
            // Arrange
            var request = new SDRequest
            {
                Id = Guid.NewGuid(),
                Prompt = "Test prompt"
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API connection failed"));

            var _sdService = new SDService(
                mockHttpClient,
                _imageRepositoryMock.Object,
                _styleRepositoryMock.Object,
                _userRepositoryMock.Object,
                _userProfileRepositoryMock.Object,
                _likeRepositoryMock.Object,
                _userServiceMock.Object,
                _configurationMock.Object);

            // Act
            var result = await _sdService.GenerateImageAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.ServiceUnavailable, result.Item1);
            Assert.Null(result.Item2);
        }

        [Fact]
        public async Task DeleteImageByIdAsync_ReturnsNoContent_WhenImageDeletedSuccessful()
        {
            // Arrange
            var imageId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var image = new Image { Id = imageId, UserId = userId, Url = "/images/test.jpg" };
            _imageRepositoryMock.Setup(repo => repo.GetImageByIdAsync(imageId))
                .ReturnsAsync(image);
            _userServiceMock.Setup(service => service.GetCurrentUserId()).Returns(userId);
            _imageRepositoryMock.Setup(repo => repo.DeleteImageByIdAsync(imageId))
                .Returns(Task.CompletedTask);
            _likeRepositoryMock.Setup(repo => repo.DeleteAllLikesByImageIdAsync(imageId))
                .Returns(Task.CompletedTask);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "test.jpg");
            File.Create(filePath).Dispose();

            // Act
            var result = await _sdService.DeleteImageByIdAsync(imageId, new ClaimsPrincipal());

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, result);
            Assert.False(File.Exists(filePath));

            _imageRepositoryMock.Verify(repo => repo.DeleteImageByIdAsync(imageId), Times.Once);
            _likeRepositoryMock.Verify(repo => repo.DeleteAllLikesByImageIdAsync(imageId), Times.Once);
        }

        [Fact]
        public async Task DeleteImageByIdAsync_ReturnsNotFound_WhenImageNotFound()
        {
            // Arrange
            var imageId = Guid.NewGuid();
            _imageRepositoryMock.Setup(repo => repo.GetImageByIdAsync(imageId))
                .ReturnsAsync((Image?)null);

            // Act
            var result = await _sdService.DeleteImageByIdAsync(imageId, new ClaimsPrincipal());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result);
        }

        [Fact]
        public async Task DeleteImageByIdAsync_ReturnsForbidden_WhenUserNotOwnerOrAdmin()
        {
            // Arrange
            var imageId = Guid.NewGuid();
            var image = new Image { Id = imageId, UserId = Guid.NewGuid(), Url = "/images/test.jpg" };
            _imageRepositoryMock.Setup(repo => repo.GetImageByIdAsync(imageId)).ReturnsAsync(image);
            _userServiceMock.Setup(service => service.GetCurrentUserId()).Returns(Guid.NewGuid());

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, Roles.User)
            }));

            // Act
            var result = await _sdService.DeleteImageByIdAsync(imageId, user);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result);
        }
    }
}