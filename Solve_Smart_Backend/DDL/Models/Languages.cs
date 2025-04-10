using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DDL.Models
{
    public class Languages
    {
        [Key]
        public int LanguagesId { get; set; }
        public string Name { get; set; }

        public ICollection<Best_Solution> Best_Solution { get; set; }
    }
}
