using ArtNaxiApi.Models.DTO;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using ArtNaxiApi.Models;
using System.Security.Claims;
using ArtNaxiApi.Constants;

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

        [Fact]
        public async Task LoginUserAsync_ReturnsOKandTokens_WhenLoginIsSuccessful()
        {
            // Arrange
            var model = new LoginDto
            {
                UsernameOrEmail = "username",
                Password = "Test123!"
            };

            var existingUser = new User
            {
                Username = "username",
                Email = "example@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                IsBanned = false
            };

            var token = "generated_token";
            var refreshToken = "refresh_token";

            _userRepositoryMock.Setup(repo => repo.GetUserByNameOrEmailAsync(model.UsernameOrEmail))
                .ReturnsAsync(existingUser);
            _jwtServiceMock.Setup(service => service.GenerateToken(It.IsAny<User>()))
                .Returns(token);
            _jwtServiceMock.Setup(service => service.GenerateRefreshToken())
               .Returns(refreshToken);
            _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(existingUser))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.LoginUserAsync(model);

            // Assert
            _userRepositoryMock.Verify(repo => repo.GetUserByNameOrEmailAsync(model.UsernameOrEmail), Times.Once);
            _jwtServiceMock.Verify(service => service.GenerateToken(It.IsAny<User>()), Times.Once);
            _jwtServiceMock.Verify(service => service.GenerateRefreshToken(), Times.Once);
            _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(existingUser), Times.Once);

            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.Equal(token, result.Item2);
            Assert.Equal(refreshToken, result.Item3);
        }

        [Fact]
        public async Task LoginUserAsync_ReturnsNotFound_WhenUsernameOrEmailIsInvalid()
        {
            // Arrange
            var model = new LoginDto
            {
                UsernameOrEmail = "invalidUsername",
                Password = "Test123!"
            };

            var existingUser = new User
            {
                Username = "username",
                Email = "example@example.com",
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByNameOrEmailAsync(model.UsernameOrEmail))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.LoginUserAsync(model);

            // Assert
            _userRepositoryMock.Verify(repo => repo.GetUserByNameOrEmailAsync(model.UsernameOrEmail), Times.Once);

            Assert.Equal(HttpStatusCode.NotFound, result.Item1);
            Assert.Null(result.Item2);
            Assert.Null(result.Item3);
        }

        [Fact]
        public async Task LoginUserAsync_ReturnsForbidden_WhenUserIsBanned()
        {
            // Arrange
            var model = new LoginDto
            {
                UsernameOrEmail = "username",
                Password = "Test123!"
            };

            var existingUser = new User
            {
                Username = "username",
                Email = "example@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                IsBanned = true
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByNameOrEmailAsync(model.UsernameOrEmail))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.LoginUserAsync(model);

            // Assert
            _userRepositoryMock.Verify(repo => repo.GetUserByNameOrEmailAsync(model.UsernameOrEmail), Times.Once);
            
            Assert.Equal(HttpStatusCode.Forbidden, result.Item1);
            Assert.Null(result.Item2);
            Assert.Null(result.Item3);
        }

        [Fact]
        public async Task LoginUserAsync_ReturnsBadRequest_WhenPasswordIsInvalid()
        {
            // Arrange
            var model = new LoginDto
            {
                UsernameOrEmail = "username",
                Password = "ivalidPassword"
            };

            var existingUser = new User
            {
                Username = "username",
                Email = "example@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password"),
                IsBanned = false
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByNameOrEmailAsync(model.UsernameOrEmail))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.LoginUserAsync(model);

            // Assert
            _userRepositoryMock.Verify(repo => repo.GetUserByNameOrEmailAsync(model.UsernameOrEmail), Times.Once);

            Assert.Equal(HttpStatusCode.BadRequest, result.Item1);
            Assert.Null(result.Item2);
            Assert.Null(result.Item3);
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsOk_WhenRefreshTokenIsValid()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var token = "valid_token";
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, RefreshToken = refreshToken };

            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Cookies["refreshToken"]).Returns(refreshToken);
            requestMock.Setup(r => r.Headers["Authorization"]).Returns(token);

            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request).Returns(requestMock.Object);
            _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(contextMock.Object);

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var principalMock = new ClaimsPrincipal(new ClaimsIdentity(claims));
            _jwtServiceMock.Setup(jwt => jwt.GetPrincipalFromExpiredToken(token)).Returns(principalMock);

            _jwtServiceMock.Setup(jwt => jwt.ValidateRefreshTokenAsync(userId, refreshToken)).ReturnsAsync(true);
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);

            var newToken = "new_token";
            var newRefreshToken = "new_refresh_token";
            _jwtServiceMock.Setup(jwt => jwt.GenerateToken(user)).Returns(newToken);
            _jwtServiceMock.Setup(jwt => jwt.GenerateRefreshToken()).Returns(newRefreshToken);

            // Act
            var result = await _userService.RefreshTokenAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.Equal(newToken, result.Item2);
            Assert.Equal(newRefreshToken, result.Item3);
            _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(It.Is<User>(u => u.RefreshToken == newRefreshToken)), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsBadRequest_WhenRefreshTokenIsMissing()
        {
            // Arrange
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request.Cookies["refreshToken"]).Returns((string)null);
            _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(contextMock.Object);

            // Act
            var result = await _userService.RefreshTokenAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.Item1);
            Assert.Null(result.Item2);
            Assert.Null(result.Item3);
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsUnauthorized_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            var refreshToken = "invalid_refresh_token";
            var token = "valid_token";
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, RefreshToken = "valid_refresh_token" };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Cookies["refreshToken"]).Returns(refreshToken);
            requestMock.Setup(r => r.Headers["Authorization"]).Returns($"Bearer {token}");

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            httpContextMock.Setup(c => c.User).Returns(userPrincipal);
            _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContextMock.Object);

            _jwtServiceMock.Setup(jwt => jwt.GetPrincipalFromExpiredToken(token))
                .Returns(userPrincipal);
            _jwtServiceMock.Setup(jwt => jwt.ValidateRefreshTokenAsync(userId, refreshToken))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.RefreshTokenAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.Item1);
            Assert.Null(result.Item2);
            Assert.Null(result.Item3);
        }

        [Fact]
        public async Task UpdateUserByIdAsync_ReturnsOK_WhenUserUpdatedSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "NewUsername",
                Email = "new@example.com",
                Password = "correctPassword",
                NewPassword = "newPassword"
            };

            var user = new User
            {
                Id = userId,
                Username = "OldUsername",
                Email = "old@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctPassword"),
                UpdatedAt = DateTime.UtcNow
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.GetUserByNameAsync(model.Username))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(model.Email))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.UpdateUserByIdAsync(userId, model, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByNameAsync(model.Username), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByEmailAsync(model.Email), Times.Once);
            _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUserByIdAsync_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "NewUsername",
                Email = "new@example.com",
                Password = "correctPassword",
                NewPassword = "newPassword"
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.User)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            // Act
            var result = await _userService.UpdateUserByIdAsync(userId, model, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result);
        }

        [Fact]
        public async Task UpdateUserByIdAsync_ReturnsNotFound_WhenUserIsNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "NewUsername",
                Email = "new@example.com",
                Password = "correctPassword",
                NewPassword = "newPassword"
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UpdateUserByIdAsync(userId, model, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task UpdateUserByIdAsync_ReturnsConflict_WhenUsernameIsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUserId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "newExistUsername",
                Email = "example@example.com",
                Password = "correctPassword",
                NewPassword = "newPassword"
            };

            var user = new User
            {
                Id = userId,
                Username = "Username",
                Email = "example@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctPassword")
            };

            var existUser = new User
            {
                Id = existingUserId,
                Username = "newExistUsername",
                Email = "different@example.com"
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.GetUserByNameAsync(model.Username))
                .ReturnsAsync(existUser);

            // Act
            var result = await _userService.UpdateUserByIdAsync(userId, model, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByNameAsync(model.Username), Times.Once);
        }

        [Fact]
        public async Task UpdateUserByIdAsync_ReturnsConflict_WhenEmailIsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUserId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "newUsername",
                Email = "exist@example.com",
                Password = "correctPassword",
                NewPassword = "newPassword"
            };

            var user = new User
            {
                Id = userId,
                Username = "Username",
                Email = "example@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctPassword")
            };

            var existUser = new User
            {
                Id = existingUserId,
                Username = "differentUsername",
                Email = "exist@example.com"
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.GetUserByNameAsync(model.Username))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(model.Email))
                .ReturnsAsync(existUser);

            // Act
            var result = await _userService.UpdateUserByIdAsync(userId, model, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByNameAsync(model.Username), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByEmailAsync(model.Email), Times.Once);
        }

        [Fact]
        public async Task UpdateUserByIdAsync_ReturnsBadRequest_WhenPasswordIsInvalid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "NewUsername",
                Email = "new@example.com",
                Password = "invalidPassword",
            };

            var user = new User
            {
                Id = userId,
                Username = "OldUsername",
                Email = "old@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctPassword"),
                UpdatedAt = DateTime.UtcNow
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.GetUserByNameAsync(model.Username))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(model.Email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UpdateUserByIdAsync(userId, model, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByNameAsync(model.Username), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByEmailAsync(model.Email), Times.Once);
        }

        [Fact]
        public async Task UpdateUserByIdAsync_ReturnsNoContent_WhenNothingToUpdate()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "Username",
                Email = "example@example.com",
                Password = "correctPassword"
            };

            var user = new User
            {
                Id = userId,
                Username = "Username",
                Email = "example@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctPassword"),
                UpdatedAt = DateTime.UtcNow
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.GetUserByNameAsync(model.Username))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(model.Email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UpdateUserByIdAsync(userId, model, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByNameAsync(model.Username), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetUserByEmailAsync(model.Email), Times.Once);
        }

        [Fact]
        public async Task UpdateUserRoleByIdAsync_ReturnsOK_WhenRoleUpdatedSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "Username",
                Email = "example@example.com",
            };
            var newRole = Roles.Admin;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(user))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.UpdateUserRoleByIdAsync(userId, newRole, userPrincipal, false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
            _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUserRoleByIdAsync_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newRole = Roles.Admin;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.User)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            // Act
            var result = await _userService.UpdateUserRoleByIdAsync(userId, newRole, userPrincipal, false);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
        }

        [Fact]
        public async Task UpdateUserRoleByIdAsync_ReturnsNotFound_WhenUserIsNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newRole = Roles.Admin;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UpdateUserRoleByIdAsync(userId, newRole, userPrincipal, false);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task UpdateUserRoleByIdAsync_ReturnsBadRequest_WhenRoleIsInvalid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "Username",
                Email = "example@example.com",
            };
            var newRole = "Invalid_role";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.UpdateUserRoleByIdAsync(userId, newRole, userPrincipal, false);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUserByIdAsync_ReturnsOK_WhenUserDeletedSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "Username",
                Email = "example@example.com",
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            _userProfileServiceMock.Setup(service => service.DeleteUserAvatarByUserIdAsync(userId, userPrincipal))
                .ReturnsAsync(HttpStatusCode.OK);
            _userRepositoryMock.Setup(repo => repo.DeleteUserAsync(user))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.DeleteUserByIdAsync(userId, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
            _userProfileServiceMock.Verify(repo => repo.DeleteUserAvatarByUserIdAsync(userId, userPrincipal), Times.Once);
            _userRepositoryMock.Verify(repo => repo.DeleteUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserByIdAsync_ReturnsBadRequest_WhenUserIsNotAdmin()
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
            var result = await _userService.DeleteUserByIdAsync(userId, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
        }

        [Fact]
        public async Task DeleteUserByIdAsync_ReturnsNotFound_WhenUserIsNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.DeleteUserByIdAsync(userId, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsOK_WhenUsersAreReturned()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "user1", Email = "user1@example.com", Role = "User", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsBanned = false },
                new User { Id = Guid.NewGuid(), Username = "user2", Email = "user2@example.com", Role = "User", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsBanned = false }
            };

            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(users);
            _userRepositoryMock.Setup(repo => repo.GetTotalCountUsersAsync()).ReturnsAsync(2);

            // Act
            var result = await _userService.GetAllUsersAsync(userPrincipal, 1, 10);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Item1);
            Assert.Equal(2, result.Item2?.Count());
            Assert.Equal(1, result.Item3);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsBadRequest_WhenUserIsNotAdmin()
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
            var result = await _userService.GetAllUsersAsync(userPrincipal, 1, 10);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.Item1);
            Assert.Null(result.Item2);
            Assert.Equal(0, result.Item3);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsNotFound_WhenUsersDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((IEnumerable<User>?)null);

            // Act
            var result = await _userService.GetAllUsersAsync(userPrincipal, 1, 10);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.Item1);
            Assert.Null(result.Item2);
            Assert.Equal(0, result.Item3);
        }

        [Fact]
        public async Task BanUnbanUserByIdAsync_ReturnsOK_WhenUserIsBannedSuccessful()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "Username",
                Email = "example@example.com",
                IsBanned = false
            };

            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);
            _imageRepositoryMock.Setup(repo => repo.SetAllUserImagesPrivateAsync(user.Id))
                .Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(user))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.BanUnbanUserByIdAsync(user.Id, true, userPrincipal);

            // Assert
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(user.Id), Times.Once);
            _imageRepositoryMock.Verify(repo => repo.SetAllUserImagesPrivateAsync(user.Id), Times.Once);
            _userRepositoryMock.Verify(service => service.UpdateUserAsync(user), Times.Once);

            Assert.Equal(HttpStatusCode.OK, result);
        }

        [Fact]
        public async Task BanUnbanUserByIdAsync_ReturnsOK_WhenUserIsUnbannedSuccessful()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "Username",
                Email = "example@example.com",
                IsBanned = true
            };

            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(user))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.BanUnbanUserByIdAsync(user.Id, true, userPrincipal);

            // Assert
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(user.Id), Times.Once);
            _userRepositoryMock.Verify(service => service.UpdateUserAsync(user), Times.Once);

            Assert.Equal(HttpStatusCode.OK, result);
        }

        [Fact]
        public async Task BanUnbanUserByIdAsync_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            var banUserId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.User)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            // Act
            var result = await _userService.BanUnbanUserByIdAsync(banUserId, true, userPrincipal);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
        }

        [Fact]
        public async Task BanUnbanUserByIdAsync_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var banUserId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin)
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(banUserId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.BanUnbanUserByIdAsync(banUserId, true, userPrincipal);

            // Assert
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(banUserId), Times.Once);
            
            Assert.Equal(HttpStatusCode.NotFound, result);
        }
    }
}