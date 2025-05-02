using Solve_Smart_Backend.DDL.Models;
using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DTOs
{
    public class ProblemDto
    {

        [Required(ErrorMessage = "عنوان المشكلة مطلوب")]
        
        public string Title { get; set; }

        [Required(ErrorMessage = "وصف المشكلة مطلوب")]
       
        public string Description { get; set; }

       
        public string? Constraints { get; set; }

        [Required(ErrorMessage = "مستوى الصعوبة مطلوب")]
        public DifficultyLevel DifficultyLevel { get; set; }

        [Required]
        public string TestCaseInput { get; set; }

        [Required]
        public string TestCaseOutput { get; set; }

        [Required]
        public string Best_Solution { get; set; }


    }
}
