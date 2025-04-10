using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DDL.Models
{
    public class UserProblem
    {
        [Key]
        public int userPId { get; set; }

        public string UserId { get; set; }
        public Users User { get; set; }

        public int ProblemId { get; set; }
        public Problem Problem { get; set; }
     
        public Submission submission { get; set; }


    }
}
