using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
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

        public AuthController(UserManager<Users> userManager, IConfiguration config, Solvedbcontext solvedbcontext)
        {
            _userManager = userManager;
            _config = config;
            _context = solvedbcontext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationDTO Rdto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(errors);
            }

            if (Rdto.Password != Rdto.ConfirmPassword)
            {
                return BadRequest("كلمات المرور غير متطابقة.");
            }

            var existingUser = await _userManager.FindByEmailAsync(Rdto.Email);
            if (existingUser != null)
            {
                return BadRequest("البريد الإلكتروني مستخدم بالفعل.");
            }

            var DbEmp = new Users()
            {
                UserName = Rdto.UserName,
                jobtitle = Rdto.Jobtitle,
                Email = Rdto.Email,
                PhoneNumber = Rdto.PhoneNumber
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
                new Claim(ClaimTypes.Role, "User")
            };

            var claimsResult = await _userManager.AddClaimsAsync(DbEmp, claims);
            if (!claimsResult.Succeeded)
            {
                var errorMessages = string.Join(", ", claimsResult.Errors.Select(e => e.Description));
                return BadRequest(errorMessages);
            }

            return Ok(new { message = "تم إنشاء المستخدم بنجاح!", userId = DbEmp.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(errors);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized("بيانات الدخول غير صحيحة");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("بيانات الدخول غير صحيحة");

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var token = GenerateJwtToken(user, roles, claims);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    jobtitle = user.jobtitle,
                    phoneNumber = user.PhoneNumber
                }
            });
        }

        private string GenerateJwtToken(Users user, IList<string> roles, IList<Claim> userClaims)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("jobtitle", user.jobtitle ?? ""),
                new Claim("phoneNumber", user.PhoneNumber ?? "")
            };

            // إضافة الأدوار
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // إضافة claims مخصصة من المستخدم
            foreach (var claim in userClaims)
            {
                if (!claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                {
                    claims.Add(claim);
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),  // تمديد وقت صلاحية التوكن
                signingCredentials: creds
            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    
    

    }
}
