namespace ArtNaxiApi.Models.DTO.Responses
{
    public class CountResponse
    {
        public int Count{ get; set; }

        public CountResponse(int count)
        {
            Count = count;
        }
    }
}
