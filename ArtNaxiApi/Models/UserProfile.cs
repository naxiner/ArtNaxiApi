using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ArtNaxiApi.Models
{
    public class UserProfile
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public User User { get; set; }

        public string? ProfilePictureUrl { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Image>? Images { get; set; } = new List<Image>();
    }
}
