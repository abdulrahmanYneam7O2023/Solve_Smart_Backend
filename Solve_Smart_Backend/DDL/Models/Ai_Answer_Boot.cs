using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DDL.Models
{
    public class Ai_Answer_Boot
    {
        [Key]
        public int AiAnswerId { get; set; }
        public string Answer { get; set; }
        public ICollection<Users_Ai> users { get; set; }
    }
}
