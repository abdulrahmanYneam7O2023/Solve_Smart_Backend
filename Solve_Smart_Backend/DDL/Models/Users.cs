using Microsoft.AspNetCore.Identity;

namespace Solve_Smart_Backend.DDL.Models
{
    public class Users : IdentityUser
    {
        public string jobtitle { get; set; }

        public ICollection<Users_Ai> users { get; set; }
        public ICollection<UserProblem> UserProblems { get; set; }
    }
}
