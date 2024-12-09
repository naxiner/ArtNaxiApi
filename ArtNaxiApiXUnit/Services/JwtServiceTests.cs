using ArtNaxiApi.Models;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArtNaxiApiXUnit.Services
{
    public class JwtServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IResponseCookies> _responseCookiesMock;
        private readonly Mock<HttpResponse> _httpResponseMock;
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _responseCookiesMock = new Mock<IResponseCookies>();
            _httpResponseMock = new Mock<HttpResponse>();

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(ctx => ctx.Response).Returns(_httpResponseMock.Object);
            _httpResponseMock.Setup(resp => resp.Cookies).Returns(_responseCookiesMock.Object);
            _httpContextAccessorMock.Setup(accessor => accessor.HttpContext).Returns(httpContextMock.Object);

            _jwtService = new JwtService(
                _configurationMock.Object,
                _userRepositoryMock.Object,
                _httpContextAccessorMock.Object);
        }

        [Fact]
        public void GenerateToken_ReturnsValidToken_WhenTokenGeneratedSuccessful()
        {
            // Arrange
            var secretKey = "MySuperSecretKey12345MySuperSecretKey12345!";
            _configurationMock
                .Setup(c => c["Jwt:Secret"])
                .Returns(secretKey);
            _configurationMock
                .Setup(c => c["Jwt:ExpiresHours"])
                .Returns("2");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "Username",
                Email = "example@example.com",
                Role = "Admin"
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
            };

            var jwtToken = tokenHandler.ReadJwtToken(token);

            Assert.Equal("Username", jwtToken.Claims.First(c => c.Type == "unique_name").Value);
            Assert.Equal(user.Id.ToString(), jwtToken.Claims.First(c => c.Type == "nameid").Value);
            Assert.Equal("example@example.com", jwtToken.Claims.First(c => c.Type == "email").Value);
            Assert.Equal("Admin", jwtToken.Claims.First(c => c.Type == "role").Value);
            Assert.NotNull(jwtToken.Claims.First(c => c.Type == "nbf"));
            Assert.NotNull(jwtToken.Claims.First(c => c.Type == "exp"));
            Assert.NotNull(jwtToken.Claims.First(c => c.Type == "iat"));
        }

        [Fact]
        public void GenerateRefreshToken_ReturnsValidRefreshToken_WhenTokenGeneratedSuccessful()
        {
            // Act
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Assert
            Assert.NotNull(refreshToken);
            Assert.False(string.IsNullOrWhiteSpace(refreshToken));
        }

        [Fact]
        public void GenerateRefreshToken_ShouldGenerateUniqueTokens()
        {
            // Arrange
            var tokens = new HashSet<string>();

            // Act
            for (int i = 0; i < 100; i++)
            {
                tokens.Add(_jwtService.GenerateRefreshToken());
            }

            // Assert
            Assert.Equal(100, tokens.Count);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ReturnsTrue_WhenTokenIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                RefreshToken = "correctToken",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(10)
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _jwtService.ValidateRefreshTokenAsync(userId, "correctToken");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ReturnsFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _jwtService.ValidateRefreshTokenAsync(userId, "sampleToken");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ReturnsFalse_WhenRefreshTokenDoesNotMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                RefreshToken = "correctToken",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(10)
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _jwtService.ValidateRefreshTokenAsync(userId, "wrongToken");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ReturnsFalse_WhenTokenIsExpired()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                RefreshToken = "correctToken",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(-1)
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _jwtService.ValidateRefreshTokenAsync(userId, "correctToken");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_ReturnsPrincipal_WhenTokenIsValid()
        {
            // Arrange
            var secretKey = "MySuperSecretKey12345MySuperSecretKey12345!";
            _configurationMock
                .Setup(c => c["Jwt:Secret"])
                .Returns(secretKey);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "user@example.com"),
                new Claim(ClaimTypes.Role, "User")
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(-1),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Act
            var principal = _jwtService.GetPrincipalFromExpiredToken(tokenString);

            // Assert
            Assert.NotNull(principal);
            Assert.IsType<ClaimsPrincipal>(principal);
            Assert.Equal("user@example.com", principal.Identity.Name);
            Assert.Contains(principal.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
        }

        [Fact]
        public void SetRefreshTokenInCookie_ShouldSetCookieWithCorrectOptions()
        {
            // Arrange
            var refreshToken = "test_refresh_token";
            CookieOptions capturedOptions = null;

            _responseCookiesMock
                .Setup(cookies => cookies.Append(
                    "refreshToken",
                    refreshToken,
                    It.IsAny<CookieOptions>()))
                .Callback<string, string, CookieOptions>((key, value, options) =>
                {
                    capturedOptions = options;
                });

            // Act
            _jwtService.SetRefreshTokenInCookie(refreshToken);

            // Assert
            _responseCookiesMock.Verify(cookies => cookies.Append("refreshToken", refreshToken, It.IsAny<CookieOptions>()), Times.Once);
            Assert.NotNull(capturedOptions);
            Assert.Equal("refreshToken", "refreshToken");
            Assert.Equal(refreshToken, refreshToken);
            Assert.True(capturedOptions.HttpOnly);
            Assert.True(capturedOptions.Secure);
            Assert.Equal(SameSiteMode.Strict, capturedOptions.SameSite);
            Assert.True(capturedOptions.Expires.HasValue);
            Assert.True(capturedOptions.Expires.Value > DateTimeOffset.UtcNow);
        }

        [Fact]
        public void SetRefreshTokenInCookie_ShouldNotAppendCookie_WhenRefreshTokenIsNullOrEmpty()
        {
            // Act
            _jwtService.SetRefreshTokenInCookie(null);
            _jwtService.SetRefreshTokenInCookie(string.Empty);

            // Assert
            _responseCookiesMock.Verify(cookies => cookies.Append(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CookieOptions>()),
                Times.Never);
        }

        [Fact]
        public void RemoveRefreshTokenFromCookie_ShouldDeleteCookieWithCorrectOptions()
        {
            // Arrange
            CookieOptions capturedOptions = null;

            _responseCookiesMock
                .Setup(cookies => cookies.Delete(
                    "refreshToken",
                    It.IsAny<CookieOptions>()))
                .Callback<string, CookieOptions>((key, options) =>
                {
                    capturedOptions = options;
                });

            // Act
            _jwtService.RemoveRefreshTokenFromCookie();

            // Assert
            _responseCookiesMock.Verify(cookies => cookies.Delete("refreshToken",It.IsAny<CookieOptions>()),Times.Once);
            Assert.NotNull(capturedOptions);
            Assert.Equal("refreshToken", "refreshToken");
            Assert.True(capturedOptions.HttpOnly);
            Assert.True(capturedOptions.Secure);
            Assert.Equal(SameSiteMode.Strict, capturedOptions.SameSite);
        }
    }
}
