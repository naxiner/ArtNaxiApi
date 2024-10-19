using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ArtNaxiApi.Validation
{
    public class PasswordComplexityAttribute : ValidationAttribute
    {
        private static readonly Regex AllowedCharsRegex =
            new Regex(@"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""|,.<>/?\\]+$");

        private static readonly Regex UpperCaseRegex = new Regex(@"[A-Z]");
        private static readonly Regex LowerCaseRegex = new Regex(@"[a-z]");
        private static readonly Regex DigitRegex = new Regex(@"\d");

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                return new ValidationResult("Password must be at least 8 characters long.");
            }

            if (!AllowedCharsRegex.IsMatch(password))
            {
                return new ValidationResult("Password must contain only letters, numbers, and special characters.");
            }

            var errors = new List<string>();

            if (!UpperCaseRegex.IsMatch(password))
                errors.Add("at least one upper case letter");
            if (!LowerCaseRegex.IsMatch(password))
                errors.Add("at least one lower case letter");
            if (!DigitRegex.IsMatch(password))
                errors.Add("at least one number");

            if (errors.Count > 0)
            {
                return new ValidationResult($"The password must contain {string.Join(", ", errors)}.");
            }

            return ValidationResult.Success!;
        }
    }
}
