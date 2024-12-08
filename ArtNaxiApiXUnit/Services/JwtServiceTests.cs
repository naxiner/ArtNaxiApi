using ArtNaxiApi.Models;
using ArtNaxiApi.Repositories;
using ArtNaxiApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ArtNaxiApiXUnit.Services
{
    public class JwtServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _userRepositoryMock = new Mock<IUserRepository>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _jwtService = new JwtService(
                _configurationMock.Object,
                _userRepositoryMock.Object,
                httpContextAccessorMock.Object);
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
    }
}
