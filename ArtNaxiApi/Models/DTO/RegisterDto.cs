﻿using ArtNaxiApi.Validation;
using System.ComponentModel.DataAnnotations;

namespace ArtNaxiApi.Models.DTO
{
    public class RegistrDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(32, ErrorMessage = "Username cannot be longer than 32 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username must contain only alphanumeric characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(32, ErrorMessage = "Email cannot be longer than 32 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [PasswordComplexity]
        public string Password { get; set; }
    }
}
