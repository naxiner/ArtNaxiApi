namespace ArtNaxiApi.Models.DTO.Responses
{
    public class RegisterResponse
    {
        public string Message { get; set; }
        public string Token { get; set; }

        public RegisterResponse(string message, string token)
        {
            Message = message;
            Token = token;
        }
    }
}
