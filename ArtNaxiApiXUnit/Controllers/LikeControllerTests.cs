using ArtNaxiApi.Controllers;
using ArtNaxiApi.Models.DTO.Responses;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace ArtNaxiApiXUnit.Controllers
{
    public class LikeControllerTests
    {
        private readonly Mock<ILikeService> _likeServiceMock;
        private readonly LikeController _likeController;

        public LikeControllerTests()
        {
            _likeServiceMock = new Mock<ILikeService>();
            _likeController = new LikeController(_likeServiceMock.Object);
        }

        [Fact]
        public async Task GetLikeCountByEntityId_ReturnsOk_WithLikeCount()
        {
            // Assert
            Guid entityId = Guid.NewGuid();
            int count = 10;

            _likeServiceMock.Setup(service => service.GetLikeCountByEntityIdAsync(entityId))
                .ReturnsAsync((HttpStatusCode.OK, count));

            // Act
            var result = await _likeController.GetLikeCountByEntityId(entityId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CountResponse>(objectResult.Value);
            Assert.Equal(count, response.Count);
        }

        [Fact]
        public async Task GetLikeStatus_ReturnsOk_WithLikeStatus()
        {
            // Assert
            Guid userId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            string entityType = "Image";
            bool isLiked = true;

            _likeServiceMock.Setup(service => service.GetLikeStatusAsync(userId, entityId, entityType))
                .ReturnsAsync((HttpStatusCode.OK, isLiked));

            // Act
            var result = await _likeController.GetLikeStatus(userId, entityId, entityType);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LikeStatusResponse>(objectResult.Value);
            Assert.Equal(isLiked, response.IsLiked);
        }

        [Fact]
        public async Task LikeEntity_ReturnsOk_WithLikeStatus()
        {
            // Assert
            Guid entityId = Guid.NewGuid();
            string entityType = "Image";

            _likeServiceMock.Setup(service => service.LikeEntityAsync(entityId, entityType))
                .ReturnsAsync((HttpStatusCode.OK));

            // Act
            var result = await _likeController.LikeEntity(entityId, entityType);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Liked successfully.", response.Message);
        }

        [Fact]
        public async Task LikeEntity_ReturnsBadRequest_WhenInvalidEntityType()
        {
            // Assert
            Guid entityId = Guid.NewGuid();
            string entityType = "InvalidType";

            _likeServiceMock.Setup(service => service.LikeEntityAsync(entityId, entityType))
                .ReturnsAsync((HttpStatusCode.BadRequest));

            // Act
            var result = await _likeController.LikeEntity(entityId, entityType);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Invalid entity type.", response.Message);
        }

        [Fact]
        public async Task LikeEntity_ReturnsConflict_WhenLikeExist()
        {
            // Assert
            Guid entityId = Guid.NewGuid();
            string entityType = "Image";

            _likeServiceMock.Setup(service => service.LikeEntityAsync(entityId, entityType))
                .ReturnsAsync((HttpStatusCode.Conflict));

            // Act
            var result = await _likeController.LikeEntity(entityId, entityType);

            // Assert
            var objectResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Already liked.", response.Message);
        }

        [Fact]
        public async Task DislikeEntity_ReturnsOk_WithLikeStatus()
        {
            // Assert
            Guid entityId = Guid.NewGuid();
            string entityType = "Image";

            _likeServiceMock.Setup(service => service.DislikeEntityAsync(entityId, entityType))
                .ReturnsAsync((HttpStatusCode.OK));

            // Act
            var result = await _likeController.DislikeEntity(entityId, entityType);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Disliked successfully.", response.Message);
        }

        [Fact]
        public async Task DislikeEntity_ReturnsBadRequest_WhenInvalidEntityType()
        {
            // Assert
            Guid entityId = Guid.NewGuid();
            string entityType = "InvalidType";

            _likeServiceMock.Setup(service => service.DislikeEntityAsync(entityId, entityType))
                .ReturnsAsync((HttpStatusCode.BadRequest));

            // Act
            var result = await _likeController.DislikeEntity(entityId, entityType);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Invalid entity type.", response.Message);
        }

        [Fact]
        public async Task DislikeEntity_ReturnsConflict_WhenLikeDoesNotExist()
        {
            // Assert
            Guid entityId = Guid.NewGuid();
            string entityType = "Image";

            _likeServiceMock.Setup(service => service.DislikeEntityAsync(entityId, entityType))
                .ReturnsAsync((HttpStatusCode.Conflict));

            // Act
            var result = await _likeController.DislikeEntity(entityId, entityType);

            // Assert
            var objectResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Like not exist.", response.Message);
        }
    }
}
