using Microsoft.AspNetCore.Identity;

namespace Solve_Smart_Backend.DDL.Models
{
    public class Users : IdentityUser
    {
     

        public ICollection<UserProblem> UserProblems { get; set; }
        
    }
}
