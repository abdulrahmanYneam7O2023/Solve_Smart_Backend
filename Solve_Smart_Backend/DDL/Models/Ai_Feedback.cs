using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DDL.Models
{
    public class Ai_Feedback
    {
        [Key]
        public int AiFeedbackId { get; set; }
        public string Feedback { get; set; }
        public int SubmissionId { get; set; }
        public Submission Submission { get; set; }
      
    }
}
