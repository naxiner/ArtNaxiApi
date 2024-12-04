using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using ArtNaxiApi.Models;

namespace ArtNaxiApiXUnit.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IImageRepository> _imageRepositoryMock;
        private readonly Mock<IUserProfileService> _userProfileServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _imageRepositoryMock = new Mock<IImageRepository>();
            _userProfileServiceMock = new Mock<IUserProfileService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _userService = new UserService(
                _userRepositoryMock.Object,
                _imageRepositoryMock.Object,
                _userProfileServiceMock.Object,
                _jwtServiceMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task RegisterUserAsync_ReturnsOKandToken_WhenRegisterIsSuccessful()
        {
            // Arrange
            var model = new RegistrDto
            {
                Username = "username",
                Email = "example@example.com",
                Password = "Test123!"
            };

            var token = "generated_token";

            _userRepositoryMock.Setup(repo => repo.GetUserByNameAsync(model.Username))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(model.Email))
                .ReturnsAsync((User?)null);
            _jwtServiceMock.Setup(service => service.GenerateToken(It.IsAny<User>()))
                .Returns(token);
            _userRepositoryMock.Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync(true);
            _userProfileServiceMock.Setup(service => service.CreateProfileAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RegisterUserAsync(model);

            // Assert
            _userRepositoryMock.Verify(repo => repo.GetUserByNameAsync(model.Username), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByEmailAsync(model.Email), Times.Once);
            _userRepositoryMock.Verify(repo => repo.AddUserAsync(It.IsAny<User>()), Times.Once);
            _userProfileServiceMock.Verify(service => service.CreateProfileAsync(It.IsAny<Guid>()), Times.Once);

            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.Equal(token, result.Item2);
        }

        [Fact]
        public async Task RegisterUserAsync_ReturnsConflict_WhenUsernameIsExist()
        {
            // Arrange
            var model = new RegistrDto
            {
                Username = "existUsername",
                Email = "example@example.com",
                Password = "Test123!"
            };

            var existingUser = new User
            {
                Username = "existUsername",
                Email = "notexistexample@example.com"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByNameAsync(model.Username))
                .ReturnsAsync(existingUser);

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(model.Email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.RegisterUserAsync(model);

            // Assert
            _userRepositoryMock.Verify(repo => repo.GetUserByNameAsync(model.Username), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByEmailAsync(model.Email), Times.Never);

            Assert.Equal(HttpStatusCode.Conflict, result.Item1);
            Assert.Null(result.Item2);
        }

        [Fact]
        public async Task RegisterUserAsync_ReturnsConflict_WhenEmailIsExist()
        {
            // Arrange
            var model = new RegistrDto
            {
                Username = "Username",
                Email = "existexample@example.com",
                Password = "Test123!"
            };

            var existingUser = new User
            {
                Username = "notexistUsername",
                Email = "existexample@example.com"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByNameAsync(model.Username))
                .ReturnsAsync((User?)null);

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(model.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.RegisterUserAsync(model);

            // Assert
            _userRepositoryMock.Verify(repo => repo.GetUserByNameAsync(model.Username), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByEmailAsync(model.Email), Times.Once);

            Assert.Equal(HttpStatusCode.Conflict, result.Item1);
            Assert.Null(result.Item2);
        }


    }
}
