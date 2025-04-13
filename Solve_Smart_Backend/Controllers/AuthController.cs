using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Solve_Smart_Backend.DDL.Models;
using Solve_Smart_Backend.DTOs;

namespace Solve_Smart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<Users> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            return Ok(new { token });
        }

        private string GenerateJwtToken(Users user, IList<string> roles)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("jobtitle", user.jobtitle ?? "")
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    
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
