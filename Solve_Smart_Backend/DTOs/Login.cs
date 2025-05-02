using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DTOs
{
    public class Login
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
