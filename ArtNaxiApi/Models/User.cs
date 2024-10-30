using ArtNaxiApi.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ArtNaxiApi.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(32)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(32)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }
        
        [Required]
        public string Role { get; set; } = Roles.User;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public UserProfile Profile { get; set; }

        [JsonIgnore]
        public string RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryDate { get; set; }
    }
}
