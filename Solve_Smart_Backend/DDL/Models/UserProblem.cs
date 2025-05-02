using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Solve_Smart_Backend.DDL.Models
{
    public class UserProblem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int ProblemId { get; set; }

        public int TriesCount { get; set; } = 0;

        public bool IsSolved { get; set; } = false;

      
        [ForeignKey("UserId")]
        public Users User { get; set; }

        [ForeignKey("ProblemId")]
        public Problem Problem { get; set; }

     
        public ICollection<Submission> Submissions { get; set; }


    }
}
