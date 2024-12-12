using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Moq;
using System.Net;
using ArtNaxiApi.Models;
using ArtNaxiApi.Constants;

namespace ArtNaxiApiXUnit.Services
{
    public class LikeServiceTests
    {
        private readonly Mock<ILikeRepository> _likeRepository;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly LikeService _likeService;
        
        public LikeServiceTests()
        {
            _likeRepository = new Mock<ILikeRepository>();
            _userServiceMock = new Mock<IUserService>();
            _likeService = new LikeService(
                _likeRepository.Object,
                _userServiceMock.Object
            );
        }

        [Fact]
        public async Task LikeEntityAsync_ReturnsOK_WhenLikedSuccessful()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entityType = EntityTypes.Image;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "user"
            };

            var like = new Like
            {
                UserId = user.Id,
                EntityId = entityId,
                EntityType = entityType
            };

            _userServiceMock.Setup(repo => repo.GetCurrentUserId()).Returns(user.Id);

            _likeRepository
                .Setup(repo => repo.IsLikeExistsAsync(user.Id, entityId, entityType))
                .ReturnsAsync(false);

            _likeRepository
                .Setup(repo => repo.LikeEntityAsync(like))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _likeService.LikeEntityAsync(entityId, entityType);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
        }

        [Fact]
        public async Task LikeEntityAsync_ReturnsBadRequest_WhenEntityTypeIsInvalid()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entityType = "invalidType";

            // Act
            var result = await _likeService.LikeEntityAsync(entityId, entityType);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
        }

        [Fact]
        public async Task LikeEntityAsync_ReturnsConflict_WhenLikeIsExist()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entityType = EntityTypes.Image;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "user"
            };

            _userServiceMock.Setup(repo => repo.GetCurrentUserId()).Returns(user.Id);

            _likeRepository
                .Setup(repo => repo.IsLikeExistsAsync(user.Id, entityId, entityType))
                .ReturnsAsync(true);

            // Act
            var result = await _likeService.LikeEntityAsync(entityId, entityType);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result);
        }

        [Fact]
        public async Task DislikeEntityAsync_ReturnsOK_WhenDislikedSuccessful()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entityType = EntityTypes.Image;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "user"
            };

            var like = new Like
            {
                UserId = user.Id,
                EntityId = entityId,
                EntityType = entityType
            };

            _userServiceMock.Setup(repo => repo.GetCurrentUserId()).Returns(user.Id);

            _likeRepository
                .Setup(repo => repo.IsLikeExistsAsync(user.Id, entityId, entityType))
                .ReturnsAsync(true);

            _likeRepository
                .Setup(repo => repo.DislikeEntityAsync(like))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _likeService.DislikeEntityAsync(entityId, entityType);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
        }

        [Fact]
        public async Task DislikeEntityAsync_ReturnsBadRequest_WhenEntityTypeIsInvalid()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entityType = "invalidType";

            // Act
            var result = await _likeService.DislikeEntityAsync(entityId, entityType);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
        }

        [Fact]
        public async Task DislikeEntityAsync_ReturnsNotFound_WhenLikeDoesNotExist()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entityType = EntityTypes.Image;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "user"
            };

            _userServiceMock.Setup(repo => repo.GetCurrentUserId()).Returns(user.Id);

            _likeRepository
                .Setup(repo => repo.IsLikeExistsAsync(user.Id, entityId, entityType))
                .ReturnsAsync(false);

            // Act
            var result = await _likeService.DislikeEntityAsync(entityId, entityType);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result);
        }
    }
}