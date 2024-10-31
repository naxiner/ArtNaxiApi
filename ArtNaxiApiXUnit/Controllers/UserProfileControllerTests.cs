using ArtNaxiApi.Controllers;
using ArtNaxiApi.Models.DTO;
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
            var userId = Guid.NewGuid();
            var userProfileDto = new UserProfileDto
            {
                Id = userId,
            };
            _userProfileServiceMock.Setup(service => service.GetProfileByUserIdAsync(userId))
                .ReturnsAsync((HttpStatusCode.OK, userProfileDto));

            var result = await _userProfileController.GetUserProfileAsync(userId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<UserProfileDto>(okResult.Value);
            Assert.Equal(userId, returnValue.Id);
        }

        [Fact]
        public async Task GetUserProfileAsync_ReturnsNotFound_WhenUserIsNotFound()
        {
            var userId = Guid.NewGuid();
            _userProfileServiceMock.Setup(service => service.GetProfileByUserIdAsync(userId))
                .ReturnsAsync((HttpStatusCode.NotFound, null));

            var result = await _userProfileController.GetUserProfileAsync(userId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User with this Id not found.", notFoundResult.Value);
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
