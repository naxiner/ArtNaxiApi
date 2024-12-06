using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ArtNaxiApiXUnit.Services
{
    public class UserProfileServiceTests
    {
        private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly UserProfileService _userProfileService;

        public UserProfileServiceTests()
        {
            _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
            _configurationMock = new Mock<IConfiguration>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _userProfileService = new UserProfileService(
                _userProfileRepositoryMock.Object,
                _configurationMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task CreateProfileAsync_ReturnsCompletedTask_WhenProfileCreatedSuccessful()
        {
            // Arrange
            var defaultAvatarUrl = "https://example.com/default-avatar.png";
            _configurationMock.Setup(config => config["FrontendSettings:DefualtAvatarUrl"])
                .Returns(defaultAvatarUrl);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "Username",
                Email = "example@example.com"
            };

            var profile = new UserProfile
            {
                UserId = user.Id,
                ProfilePictureUrl = defaultAvatarUrl
            };

            _userProfileRepositoryMock.Setup(repo => repo.AddProfileAsync(profile))
                .Returns(Task.CompletedTask);

            // Act
            await _userProfileService.CreateProfileAsync(user.Id);

            // Assert
            _userProfileRepositoryMock.Verify(repo => repo.AddProfileAsync(It.Is<UserProfile>(
                profile => profile.UserId == user.Id &&
                    profile.ProfilePictureUrl == defaultAvatarUrl
            )), Times.Once);
        }

        [Fact]
        public async Task GetProfileByUserIdAsync_ReturnsOK_WhenProfileReturnedSuccessful()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "Username",
                Email = "example@example.com"
            };

            var userProfile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                ProfilePictureUrl = "http://example.com/profile.jpg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Images = new List<Image>()
            };

            var expectedDto = new UserProfileDto
            {
                Id = userProfile.Id,
                ProfilePictureUrl = userProfile.ProfilePictureUrl,
                Images = new List<ImageDto>()
            };

            _userProfileRepositoryMock.Setup(repo => repo.GetProfileByUserIdAsync(user.Id))
                .ReturnsAsync(userProfile);

            // Act
            var (statusCode, resultDto) = await _userProfileService.GetProfileByUserIdAsync(user.Id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.NotNull(resultDto);
            Assert.Equal(expectedDto.Id, resultDto.Id);
            Assert.Equal(expectedDto.ProfilePictureUrl, resultDto.ProfilePictureUrl);
            Assert.Equal(expectedDto.Images.Count, resultDto.Images.Count);
        }

        [Fact]
        public async Task GetProfileByUserIdAsync_ReturnsNotFound_WhenProfileDoesNotExist()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "Username",
                Email = "example@example.com"
            };

            _userProfileRepositoryMock.Setup(repo => repo.GetProfileByUserIdAsync(user.Id))
                .ReturnsAsync((UserProfile?)null);

            // Act
            var result = await _userProfileService.GetProfileByUserIdAsync(user.Id);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.Item1);
            Assert.Null(result.Item2);
        }

        [Fact]
        public async Task GetProfileAvatarByUserIdAsync_ReturnsOK_WhenAvatarReturnedSuccessful()
        {
            // Arrange
            var defaultAvatarUrl = "https://example.com/default-avatar.png";
            _configurationMock.Setup(config => config["FrontendSettings:DefualtAvatarUrl"])
                .Returns(defaultAvatarUrl);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "Username",
                Email = "example@example.com"
            };

            var profile = new UserProfile
            {
                UserId = user.Id,
                ProfilePictureUrl = defaultAvatarUrl
            };

            _userProfileRepositoryMock.Setup(repo => repo.GetProfileAvatarByUserIdAsync(user.Id))
                .ReturnsAsync(defaultAvatarUrl);

            // Act
            var result = await _userProfileService.GetProfileAvatarByUserIdAsync(user.Id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.Equal(defaultAvatarUrl, result.Item2);
            _userProfileRepositoryMock.Verify(repo => repo.GetProfileAvatarByUserIdAsync(user.Id), Times.Once);
        }

        [Fact]
        public async Task GetProfileAvatarByUserIdAsync_ReturnsNotFound_WhenAvatarDoesNotExist()
        {
            // Arrange
            var defaultAvatarUrl = "https://example.com/default-avatar.png";
            _configurationMock.Setup(config => config["FrontendSettings:DefualtAvatarUrl"])
                .Returns(defaultAvatarUrl);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "Username",
                Email = "example@example.com"
            };

            var profile = new UserProfile
            {
                UserId = user.Id,
                ProfilePictureUrl = null
            };

            _userProfileRepositoryMock.Setup(repo => repo.GetProfileAvatarByUserIdAsync(user.Id))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _userProfileService.GetProfileAvatarByUserIdAsync(user.Id);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.Item1);
            Assert.Equal(defaultAvatarUrl, result.Item2);
            _userProfileRepositoryMock.Verify(repo => repo.GetProfileAvatarByUserIdAsync(user.Id), Times.Once);
        }
    }
}
