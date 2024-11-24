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
    public class StyleControllerTests
    {
        private readonly Mock<IStyleService> _styleServiceMock;
        private readonly StyleController _styleController;
        private readonly ClaimsPrincipal _user;


        public StyleControllerTests()
        {
            _styleServiceMock = new Mock<IStyleService>();
            _styleController = new StyleController(_styleServiceMock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            
            _user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
            _styleController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };
        }

        [Fact]
        public async Task GetStyleByIdAsync_ReturnsOk_WithStyle()
        {
            // Assert
            Guid styleId = Guid.NewGuid();
            var style = new StyleDto() 
            { 
                Id = styleId,
                Name = "Name"
            };

            _styleServiceMock.Setup(service => service.GetStyleByIdAsync(styleId))
                .ReturnsAsync((HttpStatusCode.OK, style));

            // Act
            var result = await _styleController.GetStyleByIdAsync(styleId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<StyleResponse>(objectResult.Value);
            Assert.Equal(style, response.Style);
        }

        [Fact]
        public async Task GetStyleByIdAsync_ReturnsNotFound_WhenStyleNotExist()
        {
            // Assert
            Guid styleId = Guid.NewGuid();
            var style = new StyleDto();

            _styleServiceMock.Setup(service => service.GetStyleByIdAsync(styleId))
                .ReturnsAsync((HttpStatusCode.NotFound, style));

            // Act
            var result = await _styleController.GetStyleByIdAsync(styleId);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Style not found.", response.Message);
        }

        [Fact]
        public async Task GetStyleByNameAsync_ReturnsOk_WithStyle()
        {
            // Assert
            Guid styleId = Guid.NewGuid();
            string styleName = "Style Name";

            var style = new StyleDto()
            {
                Id = styleId,
                Name = styleName
            };

            _styleServiceMock.Setup(service => service.GetStyleByNameAsync(styleName))
                .ReturnsAsync((HttpStatusCode.OK, style));

            // Act
            var result = await _styleController.GetStyleByNameAsync(styleName);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<StyleResponse>(objectResult.Value);
            Assert.Equal(style, response.Style);
        }

        [Fact]
        public async Task GetStyleByNameAsync_ReturnsNotFound_WhenStyleNotExist()
        {
            // Assert
            string styleName = "Style Name";
            var style = new StyleDto();

            _styleServiceMock.Setup(service => service.GetStyleByNameAsync(styleName))
                .ReturnsAsync((HttpStatusCode.NotFound, style));

            // Act
            var result = await _styleController.GetStyleByNameAsync(styleName);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Style not found.", response.Message);
        }

        [Fact]
        public async Task GetAllStylesAsync_ReturnsOk_WithAllStylesList()
        {
            // Assert
            var styles = new List<StyleDto>
            {
                new StyleDto { Id = Guid.NewGuid(), Name = "Style1" },
                new StyleDto { Id = Guid.NewGuid(), Name = "Style2" },
                new StyleDto { Id = Guid.NewGuid(), Name = "Style3" }
            };

            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(3 / (double)pageSize);

            _styleServiceMock.Setup(service => service.GetAllStylesAsync(pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.OK, styles, totalPages));

            // Act
            var result = await _styleController.GetAllStylesAsync(pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<StylesResponse>(objectResult.Value);
            Assert.Equal(styles, response.Styles);
            Assert.Equal(totalPages, response.TotalPages);
        }

        [Fact]
        public async Task GetAllStylesAsync_ReturnsNotFound_WhenStylesNotExist()
        {
            // Assert
            var styles = new List<StyleDto>();
            int pageNumber = 1, pageSize = 10;
            var totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _styleServiceMock.Setup(service => service.GetAllStylesAsync(pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.NotFound, styles, totalPages));

            // Act
            var result = await _styleController.GetAllStylesAsync(pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Styles not found.", response.Message);
        }

        [Fact]
        public async Task GetTotalStylesCountAsync_ReturnsOk_WithTotalCountStyles()
        {
            // Assert
            var styles = new List<StyleDto>
            {
                new StyleDto { Id = Guid.NewGuid(), Name = "Style1" },
                new StyleDto { Id = Guid.NewGuid(), Name = "Style2" },
                new StyleDto { Id = Guid.NewGuid(), Name = "Style3" }
            };

            int totalCount = styles.Count;

            _styleServiceMock.Setup(service => service.GetTotalStylesCountAsync())
                .ReturnsAsync((HttpStatusCode.OK, totalCount));

            // Act
            var result = await _styleController.GetTotalStylesCountAsync();

            // Arrange
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CountResponse>(objectResult.Value);
            Assert.Equal(totalCount, response.Count);
        }

        [Fact]
        public async Task GetTotalStylesCountAsync_ReturnsNotFound_WhenStylesNotExist()
        {
            // Assert
            int totalCount = 0;

            _styleServiceMock.Setup(service => service.GetTotalStylesCountAsync())
                .ReturnsAsync((HttpStatusCode.NotFound, totalCount));

            // Act
            var result = await _styleController.GetTotalStylesCountAsync();

            // Arrange
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Styles not found.", response.Message);
        }

        [Fact]
        public async Task AddStyleAsync_ReturnsOk_WhenAddedSuccessfully()
        {
            // Assert
            var style = new AddStyleDto
            {
                Name = "Style Name"
            };

            _styleServiceMock.Setup(service => service.AddStyleAsync(style, _user))
                .ReturnsAsync(HttpStatusCode.OK);

            // Act
            var result = await _styleController.AddStyleAsync(style);

            // Arrange
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Style added successfully.", response.Message);
        }

        [Fact]
        public async Task AddStyleAsync_ReturnsBadRequest_WhenUserNotAllowed()
        {
            // Assert
            var style = new AddStyleDto
            {
                Name = "Style Name"
            };

            _styleServiceMock.Setup(service => service.AddStyleAsync(style, _user))
                .ReturnsAsync(HttpStatusCode.BadRequest);

            // Act
            var result = await _styleController.AddStyleAsync(style);

            // Arrange
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("You are not allowed to add style.", response.Message);
        }

        [Fact]
        public async Task AddStyleAsync_ReturnsConflict_WhenStyleNameAlreadyExist()
        {
            // Assert
            var style = new AddStyleDto
            {
                Name = "Style Name"
            };

            _styleServiceMock.Setup(service => service.AddStyleAsync(style, _user))
                .ReturnsAsync(HttpStatusCode.Conflict);

            // Act
            var result = await _styleController.AddStyleAsync(style);

            // Arrange
            var objectResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Style with that name already exist.", response.Message);
        }

        [Fact]
        public async Task DeleteStyleByIdAsync_ReturnsNoContent_WhenStyleDeletedSuccessfully()
        {
            // Assert
            Guid styleId = Guid.NewGuid();

            _styleServiceMock.Setup(service => service.DeleteStyleByIdAsync(styleId, _user))
                .ReturnsAsync(HttpStatusCode.NoContent);

            // Act
            var result = await _styleController.DeleteStyleByIdAsync(styleId);

            // Arrange
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteStyleByIdAsync_ReturnsNotFound_WhenStyleDoesNotExist()
        {
            // Assert
            Guid styleId = Guid.NewGuid();

            _styleServiceMock.Setup(service => service.DeleteStyleByIdAsync(styleId, _user))
                .ReturnsAsync(HttpStatusCode.NotFound);

            // Act
            var result = await _styleController.DeleteStyleByIdAsync(styleId);

            // Arrange
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Style not found.", response.Message);
        }
    }
}
