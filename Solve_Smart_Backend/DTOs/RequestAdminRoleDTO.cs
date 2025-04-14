using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Solve_Smart_Backend.DTOs
{
 
    public class RequestAdminRoleDTO 
    {
        public string UserName { get; set; }
        public string Reason { get; set; }
    }
}
