using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DDL.Models
{
    public class Users_Ai
    {
        [Key]
        public int UsersAiId { get; set; }
        public string UserId { get; set; }
        public Users User { get; set; }
        public int AiAnswerId { get; set; }
        public Ai_Answer_Boot AiAnswer { get; set; }
    }
}
