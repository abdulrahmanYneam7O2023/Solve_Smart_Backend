using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DTOs
{
    public class SubmissionDto
    {
        

        [Required(ErrorMessage = "لغة البرمجة مطلوبة")]
        public int LanguageId { get; set; }

        [Required(ErrorMessage = "الكود مطلوب")]
        public string Code { get; set; }

        public string UserId { get; set; }

        [Required(ErrorMessage = "معرف المشكلة مطلوب")]
        public int ProblemId { get; set; }
    }
}
