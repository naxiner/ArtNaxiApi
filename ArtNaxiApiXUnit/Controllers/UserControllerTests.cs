using Moq;
using ArtNaxiApi.Services;
using ArtNaxiApi.Controllers;
using ArtNaxiApi.Models.DTO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

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
            var model = new RegistrDto
            {
                Username = "username",
                Email = "example@example.com",
                Password = "Test123!"
            };
            
            _userServiceMock.Setup(service => service.RegisterUserAsync(model))
                .ReturnsAsync(HttpStatusCode.OK);

            var result = await _userController.RegisterUser(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User register successful.", okResult.Value);
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
            _userServiceMock.Setup(service => service.RegisterUserAsync(It.Is<RegistrDto>(dto => dto.Username == "existingUsername")))
                    .ReturnsAsync(HttpStatusCode.Conflict);

            var model = new RegistrDto
            {
                Username = "existingUsername",
                Email = "example@example.com",
                Password = "Test123!"
            };

            var result = await _userController.RegisterUser(model);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.NotNull(conflictResult);
            Assert.Equal("User with that Username or Email already exist.", conflictResult.Value);
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
            _userServiceMock.Setup(service => service.RegisterUserAsync(It.Is<RegistrDto>(dto => dto.Email == "existing@example.com")))
                    .ReturnsAsync(HttpStatusCode.Conflict);

            var model = new RegistrDto
            {
                Username = "NewUser",
                Email = "existing@example.com",
                Password = "Test123!"
            };

            var result = await _userController.RegisterUser(model);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.NotNull(conflictResult);
            Assert.Equal("User with that Username or Email already exist.", conflictResult.Value);
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
            var model = new LoginDto
            {
                UsernameOrEmail = "example@example.com",
                Password = "Test123!"
            };

            _userServiceMock.Setup(service => service.LoginUserAsync(model))
                .ReturnsAsync((HttpStatusCode.OK, "token", "refreshToken"));

            var result = await _userController.LoginUser(model);

            var response = Assert.IsType<OkObjectResult>(result).Value;

            var tokenProperty = response.GetType().GetProperty("token");
            var refreshTokenProperty = response.GetType().GetProperty("refreshToken");

            Assert.NotNull(tokenProperty);
            Assert.NotNull(refreshTokenProperty);

            var token = tokenProperty.GetValue(response);
            var refreshToken = refreshTokenProperty.GetValue(response);

            Assert.Equal("token", token);
            Assert.Equal("refreshToken", refreshToken);
        }

        [Fact]
        public async Task Login_ReturnsNotFound_WhenUserNotFound()
        {
            var model = new LoginDto
            {
                UsernameOrEmail = "notFound@example.com",
                Password = "Test123!"
            };

            _userServiceMock.Setup(service => service.LoginUserAsync(model))
                .ReturnsAsync((HttpStatusCode.NotFound, null, null));

            var result = await _userController.LoginUser(model);

            var errorResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Invalid Username or Email.", errorResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenInvalidPassword()
        {
            var model = new LoginDto
            {
                UsernameOrEmail = "example@example.com",
                Password = "invalidPassword1!"
            };

            _userServiceMock.Setup(service => service.LoginUserAsync(model))
                .ReturnsAsync((HttpStatusCode.BadRequest, null, null));

            var result = await _userController.LoginUser(model);

            var errorResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Password.", errorResult.Value);
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
            _userServiceMock.Setup(service => service.RefreshTokenAsync())
                .ReturnsAsync((HttpStatusCode.Unauthorized, null, null));

            var result = await _userController.RefreshToken();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid refresh token.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task RefreshToken_ReturnsUnauthorized_WhenTokenIsMissing()
        {
            _userServiceMock.Setup(service => service.RefreshTokenAsync())
                .ReturnsAsync((HttpStatusCode.BadRequest, null, null));

            var result = await _userController.RefreshToken();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Refresh token is missing.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task RefreshToken_ReturnsOK_WithNewTokens_WhenSuccessful()
        {
            var newToken = "newJwtToken";
            var newRefreshToken = "newRefreshToken";

            _userServiceMock.Setup(service => service.RefreshTokenAsync())
                .ReturnsAsync((HttpStatusCode.OK, newToken, newRefreshToken));

            var result = await _userController.RefreshToken();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<OkObjectResult>(result).Value;

            var tokenProperty = response.GetType().GetProperty("token");
            var refreshTokenProperty = response.GetType().GetProperty("refreshToken");

            Assert.NotNull(tokenProperty);
            Assert.NotNull(refreshTokenProperty);

            var token = tokenProperty.GetValue(response);
            var refreshToken = refreshTokenProperty.GetValue(response);

            Assert.Equal(newToken, token);
            Assert.Equal(newRefreshToken, refreshToken);
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
            var userId = Guid.NewGuid(); 
            _userServiceMock.Setup(service => service.DeleteUserByIdAsync(userId, It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(HttpStatusCode.OK);

            var result = await _userController.DeleteUser(userId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User deleted successfully.", okResult.Value);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(service => service.DeleteUserByIdAsync(userId, It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(HttpStatusCode.NotFound);

            var result = await _userController.DeleteUser(userId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteUser_ReturnsBadRequest_WhenDeletionIsNotAllowed()
        {
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(service => service.DeleteUserByIdAsync(userId, It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(HttpStatusCode.BadRequest);

            var result = await _userController.DeleteUser(userId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("You are not allowed to delete this user.", badRequestResult.Value);
        }
        #endregion
    }
}
