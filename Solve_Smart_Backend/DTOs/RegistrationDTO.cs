using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DTOs
{
    public class RegistrationDTO
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "password is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "jobtitle is required")]
        public string jobtitle { get; set; }
    }
}
