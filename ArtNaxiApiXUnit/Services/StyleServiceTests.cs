using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Moq;
using System.Net;
using ArtNaxiApi.Models;
using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Constants;
using System.Security.Claims;

namespace ArtNaxiApiXUnit.Services
{
    public class StyleServiceTests
    {
        private readonly Mock<IStyleRepository> _styleRepositoryMock;
        private readonly StyleService _styleService;
        
        public StyleServiceTests()
        {
            _styleRepositoryMock = new Mock<IStyleRepository>();
            _styleService = new StyleService(
                _styleRepositoryMock.Object
            );
        }

        [Fact]
        public async Task GetStyleByIdAsync_ReturnsOK_WithStyleDto()
        {
            // Arrange
            var style = new Style()
            {
                Id = Guid.NewGuid(),
                Name = "Style",
                SDRequestStyles = new List<SDRequestStyle>()
            };

            var styleDto = new StyleDto()
            {
                Id = style.Id,
                Name = style.Name
            };

            _styleRepositoryMock.Setup(repo => repo.GetStyleByIdAsync(style.Id))
                .ReturnsAsync(style);

            // Act
            var result = await _styleService.GetStyleByIdAsync(style.Id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.NotNull(result.Item2);
            Assert.Equal(styleDto.Id, result.Item2?.Id);
            Assert.Equal(styleDto.Name, result.Item2?.Name);
        }

        [Fact]
        public async Task GetStyleByIdAsync_ReturnsNotFound_WhenStyleDoesNotExist()
        {
            // Arrange
            _styleRepositoryMock.Setup(repo => repo.GetStyleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Style?)null);

            // Act
            var result = await _styleService.GetStyleByIdAsync(It.IsAny<Guid>());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.Item1);
            Assert.Null(result.Item2);
        }

        [Fact]
        public async Task GetStyleByNameAsync_ReturnsOK_WithStyleDto()
        {
            // Arrange
            var style = new Style()
            {
                Id = Guid.NewGuid(),
                Name = "Style",
                SDRequestStyles = new List<SDRequestStyle>()
            };

            var styleDto = new StyleDto()
            {
                Id = style.Id,
                Name = style.Name
            };

            _styleRepositoryMock.Setup(repo => repo.GetStyleByNameAsync(style.Name))
                .ReturnsAsync(style);

            // Act
            var result = await _styleService.GetStyleByNameAsync(style.Name);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.NotNull(result.Item2);
            Assert.Equal(styleDto.Id, result.Item2?.Id);
            Assert.Equal(styleDto.Name, result.Item2?.Name);
        }

        [Fact]
        public async Task GetStyleByNameAsync_ReturnsNotFound_WhenStyleDoesNotExist()
        {
            // Arrange
            _styleRepositoryMock.Setup(repo => repo.GetStyleByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((Style?)null);

            // Act
            var result = await _styleService.GetStyleByNameAsync(It.IsAny<string>());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.Item1);
            Assert.Null(result.Item2);
        }

        [Fact]
        public async Task GetAllStylesAsync_ReturnsOK_WithListOfStyles()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 2;
            var styles = new List<Style>
            {
                new Style { Id = Guid.NewGuid(), Name = "Style 1" },
                new Style { Id = Guid.NewGuid(), Name = "Style 2" }
            };

            var totalStylesCount = 5;
            var expectedTotalPages = (int)Math.Ceiling((double)totalStylesCount / pageSize);

            _styleRepositoryMock.Setup(repo => repo.GetAllStylesAsync(pageNumber, pageSize))
                .ReturnsAsync(styles);

            _styleRepositoryMock.Setup(repo => repo.GetTotalStylesCountAsync())
                .ReturnsAsync(totalStylesCount);

            // Act
            var result = await _styleService.GetAllStylesAsync(pageNumber, pageSize);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.NotNull(result.Item2);
            Assert.Equal(styles.Count, result.Item2.Count());
            Assert.Equal(expectedTotalPages, result.Item3);

            var resultList = result.Item2.ToList();
            for (int i = 0; i < styles.Count; i++)
            {
                Assert.Equal(styles[i].Id, resultList[i].Id);
                Assert.Equal(styles[i].Name, resultList[i].Name);
            }
        }

        [Fact]
        public async Task GetAllStylesAsync_ReturnsNotFound_WhenStylesDoesNotExist()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 2;

            _styleRepositoryMock.Setup(repo => repo.GetAllStylesAsync(pageNumber, pageSize))
                .ReturnsAsync((IEnumerable<Style>?)null);

            // Act
            var result = await _styleService.GetAllStylesAsync(pageNumber, pageSize);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.Item1);
            Assert.Null(result.Item2);
            Assert.Equal(0, result.Item3);
        }

        [Fact]
        public async Task AddStyleAsync_ReturnsOK_WhenStyleAddedSuccessful()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            var addStyleDto = new AddStyleDto()
            {
                Name = "Style"
            };

            _styleRepositoryMock.Setup(repo => repo.GetStyleByNameAsync(addStyleDto.Name))
                .ReturnsAsync((Style?)null);

            var style = new Style()
            {
                Id = Guid.NewGuid(),
                Name = addStyleDto.Name
            };

            _styleRepositoryMock.Setup(repo => repo.AddStyleAsync(style))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _styleService.AddStyleAsync(addStyleDto, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
        }

        [Fact]
        public async Task AddStyleAsync_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, Roles.User)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            // Act
            var result = await _styleService.AddStyleAsync(It.IsAny<AddStyleDto>(), userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
        }

        [Fact]
        public async Task AddStyleAsync_ReturnsConflict_WhenStyleAlreadyExist()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            var addStyleDto = new AddStyleDto()
            {
                Name = "Style"
            };

            var existingStyle = new Style()
            {
                Id = Guid.NewGuid(),
                Name = addStyleDto.Name
            };

            _styleRepositoryMock.Setup(repo => repo.GetStyleByNameAsync(addStyleDto.Name))
                .ReturnsAsync(existingStyle);

            // Act
            var result = await _styleService.AddStyleAsync(addStyleDto, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result);
        }

        [Fact]
        public async Task UpdateStyleByIdAsync_ReturnsOK_WhenStyleUpdatedSuccessful()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            var styleDto = new StyleDto()
            {
                Id = Guid.NewGuid(),
                Name = "UpdatedStyle"
            };

            var existingStyle = new Style()
            {
                Id = styleDto.Id,
                Name = "Style"
            };

            _styleRepositoryMock.Setup(repo => repo.GetStyleByIdAsync(styleDto.Id))
                .ReturnsAsync(existingStyle);

            _styleRepositoryMock.Setup(repo => repo.UpdateStyleAsync(existingStyle))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _styleService.UpdateStyleByIdAsync(styleDto.Id, styleDto, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
        }

        [Fact]
        public async Task UpdateStyleByIdAsync_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, Roles.User)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            // Act
            var result = await _styleService
                .UpdateStyleByIdAsync(Guid.NewGuid(), It.IsAny<StyleDto>(), userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
        }

        [Fact]
        public async Task UpdateStyleByIdAsync_ReturnsNotFound_WhenStyleDoesNotExist()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            var styleId = Guid.NewGuid();
            _styleRepositoryMock.Setup(repo => repo.GetStyleByIdAsync(styleId))
                .ReturnsAsync((Style?)null);

            // Act
            var result = await _styleService
                .UpdateStyleByIdAsync(styleId, It.IsAny<StyleDto>(), userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result);
        }
    }
}