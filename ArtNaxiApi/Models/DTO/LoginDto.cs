using ArtNaxiApi.Validation;
using System.ComponentModel.DataAnnotations;

namespace ArtNaxiApi.Models.DTO
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username or Email is required.")]
        [StringLength(50, ErrorMessage = "Username or Email cannot be longer than 50 characters.")]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [PasswordComplexity]
        public string Password { get; set; }
    }
}
