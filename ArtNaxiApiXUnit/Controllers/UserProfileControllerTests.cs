using ArtNaxiApi.Controllers;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Models.DTO.Responses;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using System.Security.Claims;

namespace ArtNaxiApiXUnit.Controllers
{
    public class UserProfileControllerTests
    {
        private readonly UserProfileController _userProfileController;
        private readonly Mock<IUserProfileService> _userProfileServiceMock;
        private readonly ClaimsPrincipal _user;

        public UserProfileControllerTests()
        {
            _userProfileServiceMock = new Mock<IUserProfileService>();
            _userProfileController = new UserProfileController(_userProfileServiceMock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            _user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
            _userProfileController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };
        }

        [Fact]
        public async Task GetUserProfileAsync_ReturnsOk_WhenUserProfileIsFound()
        {
            // Arrange
            var userProfileDto = new UserProfileDto
            {
                Id = Guid.NewGuid(),
                Username = "username",
                Email = "email",
                ProfilePictureUrl = "avatarUrl",
                Images = []
            };

            _userProfileServiceMock.Setup(service => service.GetProfileByUserIdAsync(userProfileDto.Id))
                .ReturnsAsync((HttpStatusCode.OK, userProfileDto));

            // Act
            var result = await _userProfileController.GetUserProfileAsync(userProfileDto.Id);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<UserProfileResponse>(objectResult.Value);
            Assert.Equal(userProfileDto, response.UserProfileDto);
        }

        [Fact]
        public async Task GetUserProfileAsync_ReturnsNotFound_WhenUserIsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userProfileServiceMock.Setup(service => service.GetProfileByUserIdAsync(userId))
                .ReturnsAsync((HttpStatusCode.NotFound, null));

            // Act
            var result = await _userProfileController.GetUserProfileAsync(userId);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User with this Id not found.", response.Message);
        }
        
        [Fact]
        public async Task GetUserAvatarByIdAsync_ReturnsOk_WithProfileAvatar()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var avatarUrl = "someAvatar.png";

            _userProfileServiceMock.Setup(service => service.GetProfileAvatarByUserIdAsync(userId))
                .ReturnsAsync((HttpStatusCode.OK, avatarUrl));

            // Act
            var result = await _userProfileController.GetUserAvatarByIdAsync(userId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AvatarResponse>(objectResult.Value);
            Assert.Equal("", response.Message);
        }

        [Fact]
        public async Task GetUserAvatarByIdAsync_ReturnsNotFound_WhenAvatarIsNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var avatarUrl = "defaultAvatar.png";

            _userProfileServiceMock.Setup(service => service.GetProfileAvatarByUserIdAsync(userId))
                .ReturnsAsync((HttpStatusCode.NotFound, avatarUrl));

            // Act
            var result = await _userProfileController.GetUserAvatarByIdAsync(userId);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<AvatarResponse>(objectResult.Value);
            Assert.Equal("Avatar not found.", response.Message);
        }

        [Fact]
        public async Task UpdateProfileAvatarById_ReturnsOk_WithProfileAvatar()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("avatar.png");
            var avatarFile = fileMock.Object;

            var avatarUrl = "someAvatar.png";

            _userProfileServiceMock.Setup(service => service.UpdateProfileAvatarByUserIdAsync(userId, avatarFile))
                .ReturnsAsync((HttpStatusCode.OK, avatarUrl));

            // Act
            var result = await _userProfileController.UpdateProfileAvatarById(userId, avatarFile);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AvatarResponse>(objectResult.Value);
            Assert.Equal("Avatar updated successful.", response.Message);
            Assert.Equal(avatarUrl, response.AvatarUrl);
        }

        [Fact]
        public async Task UpdateProfileAvatarById_ReturnsForbidden_WhenUserIsNotOwner()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("avatar.png");
            var avatarFile = fileMock.Object;

            string? avatarUrl = null;

            _userProfileServiceMock.Setup(service => service.UpdateProfileAvatarByUserIdAsync(userId, avatarFile))
                .ReturnsAsync((HttpStatusCode.Forbidden, avatarUrl));

            // Act
            var result = await _userProfileController.UpdateProfileAvatarById(userId, avatarFile);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateProfileAvatarById_ReturnsBadRequest_WhenNoFileUploaded()
        {
            // Arrange
            var userId = Guid.NewGuid();

            IFormFile? avatarFile = null;

            string? avatarUrl = null;

            _userProfileServiceMock.Setup(service => service.UpdateProfileAvatarByUserIdAsync(userId, avatarFile))
                .ReturnsAsync((HttpStatusCode.BadRequest, avatarUrl));

            // Act
            var result = await _userProfileController.UpdateProfileAvatarById(userId, avatarFile);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("No file uploaded.", response.Message);
        }

        [Fact]
        public async Task DeleteUserAvatarById_ReturnsOk_WhenAvatarDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userProfileServiceMock.Setup(service => service.DeleteUserAvatarByUserIdAsync(userId, _user))
                .ReturnsAsync((HttpStatusCode.OK));

            // Act
            var result = await _userProfileController.DeleteUserAvatarById(userId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Avatar deleted successfully.", response.Message);
        }

        [Fact]
        public async Task DeleteUserAvatarById_ReturnsBadRequest_WhenUserNotOwnerOrAdmin()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userProfileServiceMock.Setup(service => service.DeleteUserAvatarByUserIdAsync(userId, _user))
                .ReturnsAsync((HttpStatusCode.BadRequest));

            // Act
            var result = await _userProfileController.DeleteUserAvatarById(userId);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("You are not allowed to delete this avatar.", response.Message);
        }

        [Fact]
        public async Task GetAllImageCountById_ReturnsOk_WithAllImagesCount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            int imageCount = 10;

            _userProfileServiceMock.Setup(service => service.GetAllImageCountByUserIdAsync(userId))
                .ReturnsAsync(imageCount);

            // Act
            var result = await _userProfileController.GetAllImageCountById(userId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CountResponse>(objectResult.Value);
            Assert.Equal(imageCount, response.Count);
        }

        [Fact]
        public async Task GetPublicImageCountById_ReturnsOk_WithAllPublicImagesCount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            int imageCount = 10;

            _userProfileServiceMock.Setup(service => service.GetPublicImageCountByUserIdAsync(userId))
                .ReturnsAsync(imageCount);

            // Act
            var result = await _userProfileController.GetPublicImageCountById(userId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CountResponse>(objectResult.Value);
            Assert.Equal(imageCount, response.Count);
        }
    }
}
