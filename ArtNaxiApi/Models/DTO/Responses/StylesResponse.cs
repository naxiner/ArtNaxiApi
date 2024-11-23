namespace ArtNaxiApi.Models.DTO.Responses
{
    public class StylesResponse
    {
        public IEnumerable<StyleDto> Styles { get; set; }
        public int TotalPages { get; set; }

        public StylesResponse(IEnumerable<StyleDto> styles, int totalPages)
        {
            Styles = styles;
            TotalPages = totalPages;
        }
    }
}
