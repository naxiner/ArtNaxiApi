using Moq;
using ArtNaxiApi.Services;
using ArtNaxiApi.Controllers;
using ArtNaxiApi.Models.DTO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using ArtNaxiApi.Models.DTO.Responses;

namespace ArtNaxiApiXUnit.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly UserController _userController;

        public UserControllerTests() 
        {
            _userServiceMock = new Mock<IUserService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _userController = new UserController(_userServiceMock.Object, _jwtServiceMock.Object);
        }

        #region REGISTER_TESTS
        [Fact]
        public async Task RegisterUser_ReturnsOK_WhenRegisterIsSuccessful()
        {
            // Arrange
            var model = new RegistrDto
            {
                Username = "username",
                Email = "example@example.com",
                Password = "Test123!"
            };

            var token = "generated_token";
            _userServiceMock.Setup(service => service.RegisterUserAsync(model))
                .ReturnsAsync((HttpStatusCode.OK, token));

            // Act
            var result = await _userController.RegisterUser(model);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RegisterResponse>(objectResult.Value);
            Assert.Equal("User register successful.", response.Message);
            Assert.Equal(token, response.Token);
        }

        [Fact]
        public async Task RegisterUser_ReturnsBadRequest_WhenUsernameIsEmpty()
        {
            var model = new RegistrDto
            {
                Username = "",
                Email = "example@example.com",
                Password = "Test123!"
            };

            _userController.ModelState.AddModelError("Username", "Username is required.");

            var result = await _userController.RegisterUser(model);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(errors.ContainsKey("Username"));
            Assert.Contains("Username is required.", (IEnumerable<string>)errors["Username"]);
        }

        [Fact]
        public async Task RegisterUser_ReturnsConflict_WhenUsernameIsExist()
        {
            // Arrange
            var model = new RegistrDto
            {
                Username = "existingUsername",
                Email = "example@example.com",
                Password = "Test123!"
            };

            _userServiceMock.Setup(service => service.RegisterUserAsync(model))
                .ReturnsAsync((HttpStatusCode.Conflict, "User with that Username or Email already exist."));

            // Act
            var result = await _userController.RegisterUser(model);

            // Assert
            var objectResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User with that Username or Email already exist.", response.Message);
        }

        [Fact]
        public async Task RegisterUser_ReturnsBadRequest_WhenEmailIsEmpty()
        {
            var model = new RegistrDto
            {
                Username = "username",
                Email = "",
                Password = "Test123!"
            };

            _userController.ModelState.AddModelError("Email", "Email is required.");
            _userController.ModelState.AddModelError("Email", "Invalid email format.");

            var result = await _userController.RegisterUser(model);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(errors.ContainsKey("Email"));
            Assert.Contains("Email is required.", (IEnumerable<string>)errors["Email"]);
            Assert.Contains("Invalid email format.", (IEnumerable<string>)errors["Email"]);
        }

        [Fact]
        public async Task RegisterUser_ReturnsConflict_WhenEmailIsExist()
        {
            // Arrange
            var model = new RegistrDto
            {
                Username = "NewUser",
                Email = "existing@example.com",
                Password = "Test123!"
            };

            _userServiceMock.Setup(service => service.RegisterUserAsync(model))
                .ReturnsAsync((HttpStatusCode.Conflict, "User with that Username or Email already exist."));

            // Act
            var result = await _userController.RegisterUser(model);

            // Assert
            var objectResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User with that Username or Email already exist.", response.Message);
        }

        [Theory]
        [InlineData("example", "Invalid email format.")]
        [InlineData("example@", "Invalid email format.")]
        [InlineData("@example", "Invalid email format.")]
        [InlineData("@example.com", "Invalid email format.")]
        [InlineData("example.com", "Invalid email format.")]
        [InlineData("example@@example.com", "Invalid email format.")]
        public async Task RegisterUser_ReturnsBadRequest_WhenEmailIsInvalid(string email, string expectedError)
        {
            var model = new RegistrDto
            {
                Username = "username",
                Email = email,
                Password = "Test123!"
            };

            _userController.ModelState.AddModelError("Email", "Invalid email format.");

            var result = await _userController.RegisterUser(model);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
            
            Assert.True(errors.ContainsKey("Email"));
            Assert.Contains(expectedError, (IEnumerable<string>)errors["Email"]);
        }

        [Fact]
        public async Task RegisterUser_ReturnsBadRequest_WhenPasswordIsEmpty()
        {
            var model = new RegistrDto
            {
                Username = "username",
                Email = "example@example.com",
                Password = ""
            };

            _userController.ModelState.AddModelError("Password", "Password is required.");
            _userController.ModelState.AddModelError("Password", "Password must be at least 8 characters long.");

            var result = await _userController.RegisterUser(model);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(errors.ContainsKey("Password"));
            Assert.Contains("Password is required.", (IEnumerable<string>)errors["Password"]);
            Assert.Contains("Password must be at least 8 characters long.", (IEnumerable<string>)errors["Password"]);
        }
        #endregion

        #region LOGIN_TESTS
        [Fact]
        public async Task Login_ReturnsOk_WhenLoginIsSuccessful()
        {
            // Arrange
            var model = new LoginDto
            {
                UsernameOrEmail = "example@example.com",
                Password = "Test123!"
            };

            _userServiceMock.Setup(service => service.LoginUserAsync(model))
                .ReturnsAsync((HttpStatusCode.OK, "token", "refreshToken"));

            // Act
            var result = await _userController.LoginUser(model);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(objectResult.Value);
            Assert.Equal("token", response.Token);
            Assert.Equal("refreshToken", response.RefreshToken);
        }

        [Fact]
        public async Task Login_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            var model = new LoginDto
            {
                UsernameOrEmail = "notFound@example.com",
                Password = "Test123!"
            };

            _userServiceMock.Setup(service => service.LoginUserAsync(model))
                .ReturnsAsync((HttpStatusCode.NotFound, null, null));

            // Act
            var result = await _userController.LoginUser(model);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Invalid Username or Email.", response.Message);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenInvalidPassword()
        {
            // Arrange
            var model = new LoginDto
            {
                UsernameOrEmail = "example@example.com",
                Password = "invalidPassword1!"
            };

            _userServiceMock.Setup(service => service.LoginUserAsync(model))
                .ReturnsAsync((HttpStatusCode.BadRequest, null, null));

            // Act
            var result = await _userController.LoginUser(model);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Invalid Password.", response.Message);
        }
        #endregion

        #region LOGOUT_TESTS
        [Fact]
        public async Task Logout_ReturnsOK_WhenLogoutSuccessful()
        {
            var result = await _userController.Logout();

            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            _jwtServiceMock.Verify(service => service.RemoveRefreshTokenFromCookie(), Times.Once);
        }
        #endregion

        #region REFRESH-TOKEN_TESTS
        [Fact]
        public async Task RefreshToken_ReturnsUnauthorized_WhenTokenIsInvalid()
        {
            // Arrange
            _userServiceMock.Setup(service => service.RefreshTokenAsync())
                .ReturnsAsync((HttpStatusCode.Unauthorized, null, null));

            // Act
            var result = await _userController.RefreshToken();

            // Assert
            var objectResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Invalid refresh token.", response.Message);
        }

        [Fact]
        public async Task RefreshToken_ReturnsUnauthorized_WhenTokenIsMissing()
        {
            // Arrange
            _userServiceMock.Setup(service => service.RefreshTokenAsync())
                .ReturnsAsync((HttpStatusCode.BadRequest, null, null));

            // Act
            var result = await _userController.RefreshToken();

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Refresh token is missing.", response.Message);
        }

        [Fact]
        public async Task RefreshToken_ReturnsOK_WithNewTokens_WhenSuccessful()
        {
            // Arrange
            var newToken = "newJwtToken";
            var newRefreshToken = "newRefreshToken";

            _userServiceMock.Setup(service => service.RefreshTokenAsync())
                .ReturnsAsync((HttpStatusCode.OK, newToken, newRefreshToken));

            // Act
            var result = await _userController.RefreshToken();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(objectResult.Value);
            Assert.Equal(newToken, response.Token);
            Assert.Equal(newRefreshToken, response.RefreshToken);
        }
        #endregion

        #region UPDATE_TESTS
        [Fact]
        public void UpdateUser_InvalidUsername_ShouldHaveValidationErrors()
        {
            var model = new UpdateUserDTO
            {
                Username = "",
                Email = "valid@example.com",
                Password = "ValidPassword123!"
            };

            var validationResults = ValidateModel(model);

            Assert.Contains(validationResults, v => v.ErrorMessage == "Username is required.");
        }

        [Fact]
        public void UpdateUser_InvalidEmail_ShouldHaveValidationErrors()
        {
            var model = new UpdateUserDTO
            {
                Username = "ValidUsername1",
                Email = "invalid-email",
                Password = "ValidPassword123!"
            };

            var validationResults = ValidateModel(model);

            Assert.Contains(validationResults, v => v.ErrorMessage == "Invalid email format.");
        }

        [Fact]
        public void UpdateUser_UsernameTooLong_ShouldHaveValidationErrors()
        {
            var model = new UpdateUserDTO
            {
                Username = new string('a', 51),
                Email = "valid@example.com",
                Password = "ValidPassword123!"
            };

            var validationResults = ValidateModel(model);

            Assert.Contains(validationResults, v => v.ErrorMessage == "Username cannot be longer than 50 characters.");
        }

        [Fact]
        public void UpdateUser_InvalidPassword_ShouldHaveValidationErrors()
        {
            var model = new UpdateUserDTO
            {
                Username = "ValidUsername1",
                Email = "valid@example.com",
                Password = "short"
            };

            var validationResults = ValidateModel(model);

            Assert.Contains(validationResults, v => v.ErrorMessage == "Password must be at least 8 characters long.");
        }

        private List<ValidationResult> ValidateModel(UpdateUserDTO model)
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, validationResults, true);
            return validationResults;
        }
        #endregion

        #region DELETE_TESTS
        [Fact]
        public async Task DeleteUser_ReturnsOk_WhenUserIsDeletedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid(); 
            _userServiceMock.Setup(service => service.DeleteUserByIdAsync(userId, It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(HttpStatusCode.OK);

            // Act
            var result = await _userController.DeleteUser(userId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User deleted successfully.", response.Message);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(service => service.DeleteUserByIdAsync(userId, It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(HttpStatusCode.NotFound);

            // Act
            var result = await _userController.DeleteUser(userId);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User not found.", response.Message);
        }

        [Fact]
        public async Task DeleteUser_ReturnsBadRequest_WhenUserNotAllowedToDelete()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(service => service.DeleteUserByIdAsync(userId, It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(HttpStatusCode.BadRequest);

            // Act
            var result = await _userController.DeleteUser(userId);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("You are not allowed to delete this user.", response.Message);
        }
        #endregion
    }
}
