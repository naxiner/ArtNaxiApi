using ArtNaxiApi.Constants;
using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;

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

        [Fact]
        public async Task UpdateProfileAvatarByUserIdAsync_ReturnsOK_WhenAvatarUpdatedSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var schemeHost = "https://example.com";
            var oldAvatarUrl = "https://example.com/avatars/old-avatar.png";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.User).Returns(userPrincipal);

            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(x => x.Scheme).Returns("https");
            requestMock.Setup(x => x.Host).Returns(new HostString("example.com"));

            httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(x => x.Length).Returns(1000);

            _userProfileRepositoryMock.Setup(repo => repo.GetProfileAvatarByUserIdAsync(userId))
                .ReturnsAsync(oldAvatarUrl);

            _userProfileRepositoryMock.Setup(repo => repo.UpdateAvatarAsync(userId, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars", $"{Guid.NewGuid()}.png");
            Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));

            formFileMock.Setup(x => x.CopyToAsync(It.IsAny<FileStream>(), default))
                .Callback<Stream, CancellationToken>((stream, token) =>
                {
                    using (var tempStream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        stream.CopyTo(tempStream);
                    }
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userProfileService.UpdateProfileAvatarByUserIdAsync(userId, formFileMock.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.NotNull(result.Item2);
            Assert.StartsWith(schemeHost, result.Item2);
            Assert.True(File.Exists(tempFilePath));

            _userProfileRepositoryMock.Verify(repo => repo.GetProfileAvatarByUserIdAsync(userId), Times.Once);
            _userProfileRepositoryMock.Verify(repo => repo.UpdateAvatarAsync(userId, It.IsAny<string>()), Times.Once);

            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task UpdateProfileAvatarByUserIdAsync_ReturnsForbidden_WhenUserIsNotOwner()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.User).Returns(userPrincipal);

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(x => x.Length).Returns(1000);

            // Act
            var result = await _userProfileService.UpdateProfileAvatarByUserIdAsync(differentUserId, formFileMock.Object);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.Item1);
            Assert.Null(result.Item2);
            _userProfileRepositoryMock.Verify(repo => repo.GetProfileAvatarByUserIdAsync(It.IsAny<Guid>()), Times.Never);
            _userProfileRepositoryMock.Verify(repo => repo.UpdateAvatarAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProfileAvatarByUserIdAsync_ReturnsBadRequest_WhenNoFileUploaded()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.User).Returns(userPrincipal);

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            // Scenario 1: Null file
            var nullFormFileMock = new Mock<IFormFile>();
            nullFormFileMock.Setup(x => x.Length).Returns(0);

            // Act
            var resultNullFile = await _userProfileService.UpdateProfileAvatarByUserIdAsync(userId, nullFormFileMock.Object);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, resultNullFile.Item1);
            Assert.Null(resultNullFile.Item2);

            // Scenario 2: Empty file
            var emptyFormFileMock = new Mock<IFormFile>();
            emptyFormFileMock.Setup(x => x.Length).Returns(0);

            // Act
            var resultEmptyFile = await _userProfileService.UpdateProfileAvatarByUserIdAsync(userId, emptyFormFileMock.Object);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, resultEmptyFile.Item1);
            Assert.Null(resultEmptyFile.Item2);

            _userProfileRepositoryMock.Verify(repo => repo.GetProfileAvatarByUserIdAsync(It.IsAny<Guid>()), Times.Never);
            _userProfileRepositoryMock.Verify(repo => repo.UpdateAvatarAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAvatarByUserIdAsync_ReturnsOK_WhenAvatarDeletedSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var schemeHost = "https://example.com";
            var currentAvatarUrl = "https://example.com/avatars/current-avatar.png";
            var defaultAvatarUrl = "https://example.com/default-avatar.png";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.User).Returns(userPrincipal);

            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(x => x.Scheme).Returns("https");
            requestMock.Setup(x => x.Host).Returns(new HostString("example.com"));

            httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            _configurationMock.Setup(x => x["FrontendSettings:DefualtAvatarUrl"])
                .Returns(defaultAvatarUrl);

            _userProfileRepositoryMock.Setup(repo => repo.GetProfileAvatarByUserIdAsync(userId))
                .ReturnsAsync(currentAvatarUrl);
            _userProfileRepositoryMock.Setup(repo => repo.UpdateAvatarAsync(userId, defaultAvatarUrl))
                .Returns(Task.CompletedTask);

            var userAvatarRelativeUrl = currentAvatarUrl.Replace(schemeHost, "");
            var userAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", userAvatarRelativeUrl.TrimStart('/'));
            Directory.CreateDirectory(Path.GetDirectoryName(userAvatarPath));
            File.WriteAllText(userAvatarPath, "Dummy avatar content");

            // Act
            var result = await _userProfileService.DeleteUserAvatarByUserIdAsync(userId, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
            Assert.False(File.Exists(userAvatarPath));
            _userProfileRepositoryMock.Verify(repo => repo.GetProfileAvatarByUserIdAsync(userId), Times.Once);
            _userProfileRepositoryMock.Verify(repo => repo.UpdateAvatarAsync(userId, defaultAvatarUrl), Times.Once);

            if (File.Exists(userAvatarPath))
            {
                File.Delete(userAvatarPath);
            }
        }

        [Fact]
        public async Task DeleteUserAvatarByUserIdAsync_ReturnsBadRequest_WhenUserDoesNotAllowedToDelete()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, differentUserId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var differentUserClaimsPrincipal = new ClaimsPrincipal(identity);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.User).Returns(differentUserClaimsPrincipal);

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            // Act
            var result = await _userProfileService.DeleteUserAvatarByUserIdAsync(userId, differentUserClaimsPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
            _userProfileRepositoryMock.Verify(repo => repo.GetProfileAvatarByUserIdAsync(It.IsAny<Guid>()), Times.Never);
            _userProfileRepositoryMock.Verify(repo => repo.UpdateAvatarAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserProfileByUserIdAsync_ReturnsOK_WhenProfileSuccessfulDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            // Act
            var result = await _userProfileService.DeleteUserProfileByUserIdAsync(userId, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
            _userProfileRepositoryMock.Verify(repo => repo.DeleteUserProfileByUserIdAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUserProfileByUserIdAsync_ReturnsBadRequest_WhenUserDoesNotAdmin()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.User)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            // Act
            var result = await _userProfileService.DeleteUserProfileByUserIdAsync(userId, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
            _userProfileRepositoryMock.Verify(repo => repo.DeleteUserProfileByUserIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
