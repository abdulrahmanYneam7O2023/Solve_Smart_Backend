using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Solve_Smart_Backend.DDL.Models
{
    public class Submission
    {
        [Key]     
        public int SId { get; set; }
        public bool? isSuccesfull { get; set; }


        public string Code { get; set; }

        public double? SuccessRate { get; set; }

        public DateTime SubmissionTime { get; set; }

        public int LanguagesId { get; set; }
    
        public string UserId { get; set; }
        public int ProblemId { get; set; }
        [ForeignKey("AiFeedback")]
        public int? AiFeedbackId { get; set; }
        public int UserProblemId { get; set; }
        public UserProblem userProblem { get; set; }
        public Languages Languages { get; set; }
        public Ai_Feedback AiFeedback { get; set; }
    }
}