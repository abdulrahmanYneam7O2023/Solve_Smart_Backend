using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DDL.Models
{
    public class Ai_Feedback
    {
        [Key]
        public int Id { get; set; }

        public int SubmissionId { get; set; }

        public bool IsCorrect { get; set; }
        public string Feedback { get; set; }

        public string CorrectSolution { get; set; }

        public Submission Submission { get; set; }
    }
}
