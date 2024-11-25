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
using ArtNaxiApi.Constants;

namespace ArtNaxiApiXUnit.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly UserController _userController;
        private readonly ClaimsPrincipal _user;

        public UserControllerTests() 
        {
            _userServiceMock = new Mock<IUserService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _userController = new UserController(_userServiceMock.Object, _jwtServiceMock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };

            _user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };
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
            // Arrange
            var model = new RegistrDto
            {
                Username = "",
                Email = "example@example.com",
                Password = "Test123!"
            };

            _userController.ModelState.AddModelError("Username", "Username is required.");

            // Act
            var result = await _userController.RegisterUser(model);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<SerializableError>(objectResult.Value);
            Assert.True(response.ContainsKey("Username"));
            Assert.Contains("Username is required.", (IEnumerable<string>)response["Username"]);
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
            // Arrange
            var model = new RegistrDto
            {
                Username = "username",
                Email = "",
                Password = "Test123!"
            };

            _userController.ModelState.AddModelError("Email", "Email is required.");
            _userController.ModelState.AddModelError("Email", "Invalid email format.");

            // Act
            var result = await _userController.RegisterUser(model);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<SerializableError>(objectResult.Value);
            Assert.True(response.ContainsKey("Email"));
            Assert.Contains("Email is required.", (IEnumerable<string>)response["Email"]);
            Assert.Contains("Invalid email format.", (IEnumerable<string>)response["Email"]);
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
            // Arrange
            var model = new RegistrDto
            {
                Username = "username",
                Email = email,
                Password = "Test123!"
            };

            _userController.ModelState.AddModelError("Email", "Invalid email format.");

            // Act
            var result = await _userController.RegisterUser(model);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<SerializableError>(objectResult.Value);
            Assert.True(response.ContainsKey("Email"));
            Assert.Contains(expectedError, (IEnumerable<string>)response["Email"]);
        }

        [Fact]
        public async Task RegisterUser_ReturnsBadRequest_WhenPasswordIsEmpty()
        {
            // Arrange
            var model = new RegistrDto
            {
                Username = "username",
                Email = "example@example.com",
                Password = ""
            };

            _userController.ModelState.AddModelError("Password", "Password is required.");
            _userController.ModelState.AddModelError("Password", "Password must be at least 8 characters long.");

            // Act
            var result = await _userController.RegisterUser(model);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<SerializableError>(objectResult.Value);
            Assert.True(response.ContainsKey("Password"));
            Assert.Contains("Password is required.", (IEnumerable<string>)response["Password"]);
            Assert.Contains("Password must be at least 8 characters long.", (IEnumerable<string>)response["Password"]);
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

        [Fact]
        public async Task Login_ReturnsForbidden_WhenUserBanned()
        {
            // Arrange
            var model = new LoginDto
            {
                UsernameOrEmail = "banned@example.com",
                Password = "Test123!"
            };

            _userServiceMock.Setup(service => service.LoginUserAsync(model))
                .ReturnsAsync((HttpStatusCode.Forbidden, null, null));

            // Act
            var result = await _userController.LoginUser(model);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
        #endregion

        #region LOGOUT_TESTS
        [Fact]
        public async Task Logout_ReturnsOK_WhenLogoutSuccessful()
        {
            // Arrange
            _jwtServiceMock.Setup(service => service.RemoveRefreshTokenFromCookie());

            // Act
            var result = await _userController.Logout();

            _jwtServiceMock.Verify(service => service.RemoveRefreshTokenFromCookie(), Times.Once);
            
            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        }
        #endregion

        #region REFRESH-TOKEN_TESTS
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
        public async Task RefreshToken_ReturnsBadRequest_WhenTokenIsMissing()
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
        #endregion

        #region UPDATE-USER_TESTS
        [Fact]
        public async Task UpdateUser_ReturnsOk_WhenUserUpdatedSuccessfylly()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "Username",
                Email = "valid@example.com",
                Password = "ValidPassword123!"
            };

            _userServiceMock.Setup(service => service.UpdateUserByIdAsync(userId, model, _user))
                .ReturnsAsync(HttpStatusCode.OK);

            // Act
            var result = await _userController.UpdateUser(userId, model);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User updated successfully.", response.Message);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var model = new UpdateUserDTO();

            _userServiceMock.Setup(service => service.UpdateUserByIdAsync(userId, model, _user))
                .ReturnsAsync(HttpStatusCode.NotFound);

            // Act
            var result = await _userController.UpdateUser(userId, model);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User not found.", response.Message);
        }

        [Fact]
        public async Task UpdateUser_ReturnsConflict_WhenUsernameOrEmailAlreadyExist()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "existUsername",
                Email = "valid@example.com",
                Password = "ValidPassword123!"
            };

            _userServiceMock.Setup(service => service.UpdateUserByIdAsync(userId, model, _user))
                .ReturnsAsync(HttpStatusCode.Conflict);

            // Act
            var result = await _userController.UpdateUser(userId, model);

            // Assert
            var objectResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Username or email already exist for another user.", response.Message);
        }

        [Fact]
        public async Task UpdateUser_ReturnsBadRequest_WhenPasswordIsInvalid()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "Username",
                Email = "valid@example.com",
                Password = "invalidPassword123!"
            };

            _userServiceMock.Setup(service => service.UpdateUserByIdAsync(userId, model, _user))
                .ReturnsAsync(HttpStatusCode.BadRequest);

            // Act
            var result = await _userController.UpdateUser(userId, model);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Invalid password.", response.Message);
        }

        [Fact]
        public async Task UpdateUser_ReturnsForbidden_WhenUserNotOwnerOrAdmin()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "Username",
                Email = "valid@example.com",
                Password = "ValidPassword123!"
            };

            _userServiceMock.Setup(service => service.UpdateUserByIdAsync(userId, model, _user))
                .ReturnsAsync(HttpStatusCode.Forbidden);

            // Act
            var result = await _userController.UpdateUser(userId, model);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNoContent_WhenNothingChanged()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "Username",
                Email = "valid@example.com",
                Password = "invalidPassword123!"
            };

            _userServiceMock.Setup(service => service.UpdateUserByIdAsync(userId, model, _user))
                .ReturnsAsync(HttpStatusCode.NoContent);

            // Act
            var result = await _userController.UpdateUser(userId, model);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUser_InvalidUsername_ShouldHaveValidationErrors()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "",
                Email = "valid@example.com",
                Password = "ValidPassword123!"
            };

            var validationResults = ValidateModel(model);

            // Act
            var result = await _userController.UpdateUser(userId, model);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            Assert.Contains(validationResults, v => v.ErrorMessage == "Username is required.");
        }

        [Fact]
        public async Task UpdateUser_InvalidEmail_ShouldHaveValidationErrors()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "ValidUsername1",
                Email = "invalid-email",
                Password = "ValidPassword123!"
            };

            var validationResults = ValidateModel(model);

            // Act
            var result = await _userController.UpdateUser(userId, model);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            Assert.Contains(validationResults, v => v.ErrorMessage == "Invalid email format.");
        }

        [Fact]
        public async Task UpdateUser_UsernameTooLong_ShouldHaveValidationErrors()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = new string('a', 51),
                Email = "valid@example.com",
                Password = "ValidPassword123!"
            };

            var validationResults = ValidateModel(model);

            // Act
            var result = await _userController.UpdateUser(userId, model);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            Assert.Contains(validationResults, v => v.ErrorMessage == "Username cannot be longer than 50 characters.");
        }

        [Fact]
        public async Task UpdateUser_InvalidPassword_ShouldHaveValidationErrors()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            var model = new UpdateUserDTO
            {
                Username = "ValidUsername1",
                Email = "valid@example.com",
                Password = "short"
            };

            var validationResults = ValidateModel(model);

            // Act
            var result = await _userController.UpdateUser(userId, model);

            // Assert
            Assert.IsType<BadRequestResult>(result);
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

        #region UPDATE-USER-ROLE_TESTS
        [Fact]
        public async Task UpdateUserRole_ReturnsOk_WhenUserRoleUpdatedSuccessfylly()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            string newRole = Roles.User;

            _userServiceMock.Setup(service => service.UpdateUserRoleByIdAsync(userId, newRole, _user, false))
                .ReturnsAsync(HttpStatusCode.OK);

            // Act
            var result = await _userController.UpdateUserRole(userId, newRole);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Role has been successfully assigned.", response.Message);
        }

        [Fact]
        public async Task UpdateUserRole_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            string newRole = Roles.User;

            _userServiceMock.Setup(service => service.UpdateUserRoleByIdAsync(userId, newRole, _user, false))
                .ReturnsAsync(HttpStatusCode.BadRequest);

            // Act
            var result = await _userController.UpdateUserRole(userId, newRole);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("You are not allowed to set this role.", response.Message);
        }

        [Fact]
        public async Task UpdateUserRole_ReturnsNotFound_WhenUserIsNotAdmin()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            string newRole = Roles.User;

            _userServiceMock.Setup(service => service.UpdateUserRoleByIdAsync(userId, newRole, _user, false))
                .ReturnsAsync(HttpStatusCode.NotFound);

            // Act
            var result = await _userController.UpdateUserRole(userId, newRole);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User not found.", response.Message);
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
        #endregion

        #region GET-ALL-USERS_TESTS
        [Fact]
        public async Task GetAllUsers_ReturnsOk_WithAllUsersList()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = Guid.NewGuid(), Username = "Username1", Email = "example1@gmail.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Role = Roles.User, IsBanned = false },
                new UserDto { Id = Guid.NewGuid(), Username = "Username2", Email = "example2@gmail.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Role = Roles.Admin, IsBanned = false },
                new UserDto { Id = Guid.NewGuid(), Username = "Username3", Email = "example3@gmail.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Role = Roles.User, IsBanned = true },
            };

            var userId = Guid.NewGuid();
            int pageNumber = 1, pageSize = 10;
            int totalPages = (int)Math.Ceiling(3 / (double)pageSize);

            _userServiceMock.Setup(service => service.GetAllUsersAsync(_user, pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.OK, users, totalPages));

            // Act
            var result = await _userController.GetAllUsers(pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<UsersResponse>(objectResult.Value);
            Assert.Equal(users, response.Users);
            Assert.Equal(totalPages, response.TotalPages);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            var users = new List<UserDto>();
            var userId = Guid.NewGuid();
            int pageNumber = 1, pageSize = 10;
            int totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _userServiceMock.Setup(service => service.GetAllUsersAsync(_user, pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.BadRequest, users, totalPages));

            // Act
            var result = await _userController.GetAllUsers(pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("You are not allowed to get all users.", response.Message);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var users = new List<UserDto>();
            var userId = Guid.NewGuid();
            int pageNumber = 1, pageSize = 10;
            int totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _userServiceMock.Setup(service => service.GetAllUsersAsync(_user, pageNumber, pageSize))
                .ReturnsAsync((HttpStatusCode.NotFound, users, totalPages));

            // Act
            var result = await _userController.GetAllUsers(pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Users not found.", response.Message);
        }
        #endregion

        #region GET-USERS-BY-QUERY_TESTS
        [Fact]
        public async Task GetUsersByQuery_ReturnsOk_WithAllUsersFoundList()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = Guid.NewGuid(), Username = "Username1", Email = "example1@gmail.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Role = Roles.User, IsBanned = false },
                new UserDto { Id = Guid.NewGuid(), Username = "Username2", Email = "example2@gmail.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Role = Roles.Admin, IsBanned = false },
                new UserDto { Id = Guid.NewGuid(), Username = "Username3", Email = "example3@gmail.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Role = Roles.User, IsBanned = true },
            };

            string query = "User";
            int pageNumber = 1, pageSize = 10;
            int totalPages = (int)Math.Ceiling(3 / (double)pageSize);

            _userServiceMock.Setup(service => service.SearchUsersAsync(query, pageNumber, pageSize, _user))
                .ReturnsAsync((HttpStatusCode.OK, users, totalPages));

            // Act
            var result = await _userController.GetUsersByQuery(query, pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<UsersResponse>(objectResult.Value);
            Assert.Equal(users, response.Users);
            Assert.Equal(totalPages, response.TotalPages);
        }

        [Fact]
        public async Task GetUsersByQuery_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            var users = new List<UserDto>();
            string query = "User";
            int pageNumber = 1, pageSize = 10;
            int totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _userServiceMock.Setup(service => service.SearchUsersAsync(query, pageNumber, pageSize, _user))
                .ReturnsAsync((HttpStatusCode.BadRequest, users, totalPages));

            // Act
            var result = await _userController.GetUsersByQuery(query, pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("You are not allowed to search users.", response.Message);
        }

        [Fact]
        public async Task GetUsersByQuery_ReturnsNotFound_WhenUsersDoesNotExist()
        {
            // Arrange
            var users = new List<UserDto>();
            string query = "User";
            int pageNumber = 1, pageSize = 10;
            int totalPages = (int)Math.Ceiling(0 / (double)pageSize);

            _userServiceMock.Setup(service => service.SearchUsersAsync(query, pageNumber, pageSize, _user))
                .ReturnsAsync((HttpStatusCode.NotFound, users, totalPages));

            // Act
            var result = await _userController.GetUsersByQuery(query, pageNumber, pageSize);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("Users not found.", response.Message);
        }
        #endregion

        #region BAN-USER_TESTS
        [Fact]
        public async Task BanUser_ReturnsOk_WhenUserBannedSuccessfully()
        {
            // Arrange
            Guid userId = Guid.NewGuid();           

            _userServiceMock.Setup(service => service.BanUnbanUserByIdAsync(userId, true, _user))
                .ReturnsAsync(HttpStatusCode.OK);

            // Act
            var result = await _userController.BanUser(userId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User banned successfully.", response.Message);
        }

        [Fact]
        public async Task BanUser_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            Guid userId = Guid.NewGuid();

            _userServiceMock.Setup(service => service.BanUnbanUserByIdAsync(userId, true, _user))
                .ReturnsAsync(HttpStatusCode.BadRequest);

            // Act
            var result = await _userController.BanUser(userId);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("You are not allowed to Ban this user.", response.Message);
        }

        [Fact]
        public async Task BanUser_ReturnsNotFound_WhenUserIsNotExist()
        {
            // Arrange
            Guid userId = Guid.NewGuid();

            _userServiceMock.Setup(service => service.BanUnbanUserByIdAsync(userId, true, _user))
                .ReturnsAsync(HttpStatusCode.NotFound);

            // Act
            var result = await _userController.BanUser(userId);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User not found.", response.Message);
        }
        #endregion

        #region UNBAN-USER_TESTS
        [Fact]
        public async Task UnbanUser_ReturnsOk_WhenUserUnbannedSuccessfully()
        {
            // Arrange
            Guid userId = Guid.NewGuid();

            _userServiceMock.Setup(service => service.BanUnbanUserByIdAsync(userId, false, _user))
                .ReturnsAsync(HttpStatusCode.OK);

            // Act
            var result = await _userController.UnbanUser(userId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User unbanned successfully.", response.Message);
        }

        [Fact]
        public async Task UnbanUser_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            Guid userId = Guid.NewGuid();

            _userServiceMock.Setup(service => service.BanUnbanUserByIdAsync(userId, false, _user))
                .ReturnsAsync(HttpStatusCode.BadRequest);

            // Act
            var result = await _userController.UnbanUser(userId);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("You are not allowed to Unban this user.", response.Message);
        }

        [Fact]
        public async Task UnbanUser_ReturnsNotFound_WhenUserIsNotExist()
        {
            // Arrange
            Guid userId = Guid.NewGuid();

            _userServiceMock.Setup(service => service.BanUnbanUserByIdAsync(userId, false, _user))
                .ReturnsAsync(HttpStatusCode.NotFound);

            // Act
            var result = await _userController.UnbanUser(userId);

            // Assert
            var objectResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(objectResult.Value);
            Assert.Equal("User not found.", response.Message);
        }
        #endregion
    }
}
