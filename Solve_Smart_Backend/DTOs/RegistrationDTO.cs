using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DTOs
{
    public class RegistrationDTO
    {
        [Required(ErrorMessage = "UserName is required")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Name can only contain letters, numbers, and spaces.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "password is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "jobtitle is required")]

      
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        public string Jobtitle { get; set; }
    }
}
