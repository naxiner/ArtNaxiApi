using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Moq;
using System.Net;
using ArtNaxiApi.Models;
using System.Security.Claims;
using ArtNaxiApi.Constants;

namespace ArtNaxiApiXUnit.Services
{
    public class ImageServiceTests
    {
        private readonly Mock<IImageRepository> _imageRepositoryMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ISDService> _sdServiceMock;
        private readonly ImageService _imageService;
        
        public ImageServiceTests()
        {
            _imageRepositoryMock = new Mock<IImageRepository>();
            _userServiceMock = new Mock<IUserService>();
            _sdServiceMock = new Mock<ISDService>();
            _imageService = new ImageService(
                _imageRepositoryMock.Object,
                _sdServiceMock.Object,
                _userServiceMock.Object
            );
        }

        [Fact]
        public async Task GetAllImagesAsync_ReturnsOK_WithAllImages()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin"
            };

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, Roles.Admin)
            }));

            var mockImages = new List<Image>
            {
                new Image { Id = Guid.NewGuid(), Url = "http://example.com/image1.jpg", CreatedBy = user.Username, IsPublic = false, UserId = user.Id, User = user, Request = new SDRequest() },
                new Image { Id = Guid.NewGuid(), Url = "http://example.com/image2.jpg", CreatedBy = user.Username, IsPublic = false, UserId = user.Id, User = user, Request = new SDRequest() }
            };

            _imageRepositoryMock
                .Setup(repo => repo.GetAllImagesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(mockImages);

            _imageRepositoryMock
                .Setup(repo => repo.GetTotalImagesCountAsync())
                .ReturnsAsync(2);

            // Act
            var result = await _imageService.GetAllImagesAsync(1, 10, userClaims);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.NotEmpty(result.Item2);
            Assert.Equal(2, result.Item2.Count());
            Assert.Equal(1, result.Item3);
        }

        [Fact]
        public async Task GetAllImagesAsync_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, Roles.User)
            }));

            // Act
            var result = await _imageService.GetAllImagesAsync(1, 10, userClaims);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.Item1);
            Assert.Empty(result.Item2);
            Assert.Equal(0, result.Item3);
        }

        [Fact]
        public async Task GetAllImagesAsync_ReturnsNotFound_WhenImagesDoesNotExist()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin"
            };

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, Roles.Admin)
            }));

            _imageRepositoryMock
                .Setup(repo => repo.GetAllImagesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((IEnumerable<Image>?)null);

            _imageRepositoryMock
                .Setup(repo => repo.GetTotalImagesCountAsync())
                .ReturnsAsync(0);

            // Act
            var result = await _imageService.GetAllImagesAsync(1, 10, userClaims);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.Item1);
            Assert.Empty(result.Item2);
            Assert.Equal(0, result.Item3);
        }

        [Fact]
        public async Task GetImageByIdAsync_ReturnsOK_WithImageDto()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin"
            };

            var mockImage = new Image { Id = Guid.NewGuid(), Url = "http://example.com/image1.jpg", CreatedBy = user.Username, IsPublic = false, UserId = user.Id, User = user, Request = new SDRequest() };
                
            _imageRepositoryMock
                .Setup(repo => repo.GetImageByIdAsync(mockImage.Id))
                .ReturnsAsync(mockImage);

            // Act
            var result = await _imageService.GetImageByIdAsync(mockImage.Id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.NotNull(result.Item2);
        }

        [Fact]
        public async Task GetImageByIdAsync_ReturnsNotFound_WhenImageDoesNotExist()
        {
            // Arrange
            _imageRepositoryMock
                .Setup(repo => repo.GetImageByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Image?)null);

            // Act
            var result = await _imageService.GetImageByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.Item1);
            Assert.Null(result.Item2);
        }

        [Fact]
        public async Task GetImagesByUserIdAsync_ReturnsOK_WhenImagesExist()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "user"
            };

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, Roles.Admin)
            }));

            var mockImages = new List<Image>
            {
                new Image { Id = Guid.NewGuid(), Url = "http://example.com/image1.jpg", CreatedBy = user.Username, IsPublic = false, UserId = user.Id, User = user, Request = new SDRequest() },
                new Image { Id = Guid.NewGuid(), Url = "http://example.com/image2.jpg", CreatedBy = user.Username, IsPublic = false, UserId = user.Id, User = user, Request = new SDRequest() }
            };

            _imageRepositoryMock
                .Setup(repo => repo.GetImagesByUserIdAsync(
                    user.Id, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(mockImages);

            _imageRepositoryMock
                .Setup(repo => repo.GetTotalImagesCountByUserIdAsync(user.Id))
                .ReturnsAsync(2);

            // Act
            var result = await _imageService.GetImagesByUserIdAsync(user.Id, 1, 10, userClaims);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.NotEmpty(result.Item2);
            Assert.Equal(2, result.Item2.Count());
            Assert.Equal(1, result.Item3);
        }

        [Fact]
        public async Task GetImagesByUserIdAsync_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, Roles.User)
            }));

            // Act
            var result = await _imageService.GetImagesByUserIdAsync(Guid.NewGuid(), 1, 10, userClaims);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.Item1);
            Assert.Empty(result.Item2);
            Assert.Equal(0, result.Item3);
        }

        [Fact]
        public async Task GetImagesByUserIdAsync_ReturnsNotFound_WhenImagesDoesNotExist()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "user"
            };

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, Roles.Admin)
            }));

            _imageRepositoryMock
                .Setup(repo => repo.GetImagesByUserIdAsync(
                    user.Id, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((IEnumerable<Image>?)null);

            _imageRepositoryMock
                .Setup(repo => repo.GetTotalImagesCountByUserIdAsync(user.Id))
                .ReturnsAsync(0);

            // Act
            var result = await _imageService.GetImagesByUserIdAsync(user.Id, 1, 10, userClaims);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.Item1);
            Assert.Empty(result.Item2);
            Assert.Equal(0, result.Item3);
        }
    }
}