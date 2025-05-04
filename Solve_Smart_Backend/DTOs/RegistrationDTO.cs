using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DTOs
{
    public class RegistrationDTO
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "يجب أن يكون اسم المستخدم بين 3 و 50 حرفًا")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "اسم المستخدم يمكن أن يحتوي على أحرف وأرقام ومسافات فقط")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "يجب أن تكون كلمة المرور 6 أحرف على الأقل")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
            ErrorMessage = "يجب أن تحتوي كلمة المرور على حرف كبير وحرف صغير ورقم واحد على الأقل")]
        public string Password { get; set; }


        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string PhoneNumber { get; set; }
    }
}
