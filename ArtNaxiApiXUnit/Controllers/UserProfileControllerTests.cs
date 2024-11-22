using ArtNaxiApi.Controllers;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Models.DTO.Responses;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace ArtNaxiApiXUnit.Controllers
{
    public class UserProfileControllerTests
    {
        private readonly UserProfileController _userProfileController;
        private readonly Mock<IUserProfileService> _userProfileServiceMock;

        public UserProfileControllerTests()
        {
            _userProfileServiceMock = new Mock<IUserProfileService>();
            _userProfileController = new UserProfileController(_userProfileServiceMock.Object);
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
        public async Task GetUserProfileAsync_ReturnsBadRequest_WhenOtherErrorOccurs()
        {
            var userId = Guid.NewGuid();
            _userProfileServiceMock.Setup(service => service.GetProfileByUserIdAsync(userId))
                .ReturnsAsync((HttpStatusCode.BadRequest, null));

            var result = await _userProfileController.GetUserProfileAsync(userId);

            Assert.IsType<BadRequestResult>(result);
        }
    }
}
