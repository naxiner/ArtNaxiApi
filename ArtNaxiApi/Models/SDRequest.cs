using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ArtNaxiApi.Models
{
    public class SDRequest
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        [Required]
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("negative_prompt")]
        public string? NegativePrompt { get; set; } = string.Empty;

        [JsonPropertyName("styles")]
        public List<string>? Styles { get; set; } = new List<string>();

        [Required]
        [JsonPropertyName("sampler_name")]
        public string SamplerName { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("scheduler")]
        public string Scheduler { get; set; } = "automatic";

        [Required]
        [JsonPropertyName("steps")]
        public int Steps { get; set; }

        [Required]
        [JsonPropertyName("cfg_scale")]
        public int CfgScale { get; set; }

        [Required]
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [Required]
        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonIgnore]
        public Guid? ImageId { get; set; }

        [JsonIgnore]
        public Image? Image { get; set; }
    }
}
