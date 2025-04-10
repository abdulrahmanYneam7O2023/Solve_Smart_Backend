using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DDL.Models
{
    public class Best_Solution
    {
        [Key]
        public int best_SID { get; set; }
        public string Code { get; set; }

        public string Memory { get; set; }
        public string Runtime { get; set; }

       
        public int ProgramId { get; set; }
        public int languageId { get; set; }
        public Problem Problem { get; set; }
        public Languages languages { get; set; }


    }
}
