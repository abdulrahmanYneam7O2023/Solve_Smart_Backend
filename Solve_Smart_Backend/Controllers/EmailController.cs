using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Solve_Smart_Backend.DTOs;

namespace Solve_Smart_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailSender _emailSender;

        public EmailController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        [Authorize]
        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            await _emailSender.SendEmailAsync(
               
                request.UserEmail,
                request.Subject,
                request.Message
            );

            return Ok();
        }


    }
}
