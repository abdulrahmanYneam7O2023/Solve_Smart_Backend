using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DDL.Models
{
    public class Submission
    {
        [Key]     
        public int SId { get; set; }
        public bool isSuccesfull { get; set; }
        public int SuccessRate { get; set; }
        public DateTime SubmissionTime { get; set; }
        
        public int LanguagesId { get; set; }
        public int AiFeedbackId { get; set; }
    
        public string UserId { get; set; }
        public int ProblemId { get; set; }
        public UserProblem userProblem { get; set; }
        public Languages Languages { get; set; }
        public Ai_Feedback Ai_Feedback { get; set; }
    }
}
