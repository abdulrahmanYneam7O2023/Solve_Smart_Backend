namespace Solve_Smart_Backend.DDL.Models
{
    public class AdminRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
        public DateTime RequestedAt { get; set; }  
        public string Reason { get; set; } 

        public Users User { get; set; }
    }
}
