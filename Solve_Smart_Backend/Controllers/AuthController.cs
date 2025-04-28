using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Solve_Smart_Backend.DDL.Context;
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
        private readonly Solvedbcontext _context;

        public AuthController(UserManager<Users> userManager, IConfiguration config , Solvedbcontext solvedbcontext)
        {
            _userManager = userManager;
            _config = config;
            _context = solvedbcontext;
        }
        [HttpPost("register")]
        public async Task<IActionResult> register([FromBody] RegistrationDTO Rdto)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(errors);

            if (Rdto.Password != Rdto.ConfirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

        
            var existingUser = await _userManager.FindByEmailAsync(Rdto.Email);
            if (existingUser != null)
            {
                return BadRequest("Email is already taken.");
            }

            
            var DbEmp = new Users()
            {
                UserName = Rdto.UserName,
                jobtitle = Rdto.Jobtitle,
                Email = Rdto.Email,
            };

            var res = await _userManager.CreateAsync(DbEmp, Rdto.Password);
            if (!res.Succeeded)
            {
                var errorMessages = string.Join(", ", res.Errors.Select(e => e.Description));
                return BadRequest(errorMessages);
            }

            
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, DbEmp.Id),
        new Claim(ClaimTypes.Role, "Admin")
    };

            var claimsResult = await _userManager.AddClaimsAsync(DbEmp, claims);
            if (!claimsResult.Succeeded)
            {
                var errorMessages = string.Join(", ", claimsResult.Errors.Select(e => e.Description));
                return BadRequest(errorMessages);
            }

            // إرجاع استجابة ناجحة عند إنشاء المستخدم
            return Ok(new { message = "User created successfully!" });
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

    
    

    }
}
