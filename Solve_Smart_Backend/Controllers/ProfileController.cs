using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Solve_Smart_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
      
            [Authorize]
            [HttpGet("me")]
            public IActionResult GetProfile()
            {
                var username = User.Identity?.Name;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var jobtitle = User.FindFirst("jobtitle")?.Value;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                return Ok(new
                {
                    Id = userId,
                    Username = username,
                    Email = email,
                    JobTitle = jobtitle,
                    Roles = roles
                });
            }
        }
 
    }