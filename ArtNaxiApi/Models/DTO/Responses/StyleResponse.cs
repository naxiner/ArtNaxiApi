namespace ArtNaxiApi.Models.DTO.Responses
{
    public class StyleResponse
    {
        public StyleDto Style { get; set; }

        public StyleResponse(StyleDto style)
        {
            Style = style;
        }
    }
}
