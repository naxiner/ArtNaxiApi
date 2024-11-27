namespace ArtNaxiApi.Models.DTO
{
    public class SDRequestDto
    {
        public Guid Id { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string? NegativePrompt { get; set; } = string.Empty;
        public List<string>? Styles { get; set; } = new List<string>();
        public int Seed { get; set; } = -1;
        public string SamplerName { get; set; } = string.Empty;
        public string Scheduler { get; set; } = "automatic";
        public int Steps { get; set; }
        public int CfgScale { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
