using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solve_Smart_Backend.DDL.Context;
using Solve_Smart_Backend.DDL.Models;
using Solve_Smart_Backend.DTOs;

namespace Solve_Smart_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")] 
    public class AdminController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly Solvedbcontext _context;

        public AdminController(UserManager<Users> userManager, Solvedbcontext context)
        {
            _userManager = userManager;
            _context = context;
        }

      

       
        [HttpGet("get-admin-requests")]
        public IActionResult GetAdminRequests()
        {
            var requests = _context.adminRequests.Where(r => r.Status == "Pending").ToList();
            return Ok(requests);
        }


        [HttpPost("approve-admin-request")]
        public async Task<IActionResult> ApproveAdminRequest([FromBody] ApproveAdminRequestDTO approveRequest)
        {
         

            var adminRequest = await _context.adminRequests
                .Include(ar => ar.User) 
                .FirstOrDefaultAsync(ar => ar.User.UserName == approveRequest.UserName);

            if (adminRequest == null)
            {
                return NotFound("Admin request not found.");
            }

            var user = await _userManager.FindByNameAsync(approveRequest.UserName);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (approveRequest.IsApproved)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                adminRequest.Status = "Approved";
            }
            else
            {
                adminRequest.Status = "Rejected";
            }

            _context.adminRequests.Update(adminRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Request processed successfully." });
        }

    }
}
