using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DDL.Models
{
    public class TestCases
    {
        [Key]
        public int TestCaseId { get; set; }
        public string TestCaseName { get; set; }
        public string TestCaseDescription { get; set; }
  
        public string TestCaseOutput { get; set; }
       public int problemId { get; set; }
        public Problem problems { get; set; }
      
    }
}
