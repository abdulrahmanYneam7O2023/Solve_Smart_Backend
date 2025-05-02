using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DDL.Models
{
    public class Languages
    {
        [Key]
        public int LanguagesId { get; set; }
        public string Name { get; set; }


        public int submissionId { get; set; }
        public Submission submission { get; set; }
    }
}
