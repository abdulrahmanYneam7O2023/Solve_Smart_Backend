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
        private readonly IConfiguration _configuration;
        private readonly UserManager<Users> _userManager;
        private readonly Solvedbcontext _context;

        public AuthController(IConfiguration configuration, UserManager<Users> userManager, Solvedbcontext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<TokenResponseDTO>> Login(Login credentials)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(errors);
            }

            var user = await _userManager.FindByEmailAsync(credentials.Email);
            if (user == null)
            {
                return Unauthorized("بيانات الدخول غير صحيحة");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return BadRequest("الحساب مغلق مؤقتاً، يرجى المحاولة لاحقاً");
            }

            bool isAuthenticated = await _userManager.CheckPasswordAsync(user, credentials.Password);
            if (!isAuthenticated)
            {
                await _userManager.AccessFailedAsync(user);
                return Unauthorized("بيانات الدخول غير صحيحة");
            }

          
            await _userManager.ResetAccessFailedCountAsync(user);

           
            var userRoles = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);

            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("phoneNumber", user.PhoneNumber ?? "")
            };

           
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

       
            foreach (var claim in userClaims)
            {
                if (!claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                {
                    claims.Add(claim);
                }
            }

          
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"];
            var secretKeyInBytes = Encoding.UTF8.GetBytes(secretKey);
            var key = new SymmetricSecurityKey(secretKeyInBytes);

        
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenValidityMinutes = int.Parse(jwtSettings["TokenvalidityMinutes"] ?? "30");
            var expiryTime = DateTime.Now.AddMinutes(tokenValidityMinutes);

           
            var jwt = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                notBefore: DateTime.Now,
                expires: expiryTime,
                signingCredentials: signingCredentials
            );

           
            var tokenHandler = new JwtSecurityTokenHandler();
            string tokenString = tokenHandler.WriteToken(jwt);

          
            return Ok(new TokenResponseDTO
            {
                Token = tokenString,
                ExpiryDate = expiryTime,
                
            });
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register(RegistrationDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(errors);
            }

            var existingUser = await _userManager.FindByEmailAsync(registerDTO.Email);
            if (existingUser != null)
            {
                return BadRequest("البريد الإلكتروني مستخدم بالفعل.");
            }

            var newUser = new Users
            {
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber,
                EmailConfirmed = true 
            };

            var creationResult = await _userManager.CreateAsync(newUser, registerDTO.Password);
            if (!creationResult.Succeeded)
            {
                var errorMessages = string.Join(", ", creationResult.Errors.Select(e => e.Description));
                return BadRequest(errorMessages);
            }

          
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.Id),
                new Claim(ClaimTypes.Email, newUser.Email),
                new Claim(ClaimTypes.Name, newUser.UserName),
                  new Claim(ClaimTypes.Role, registerDTO.UserName),
            };

           
            var claimsResult = await _userManager.AddClaimsAsync(newUser, userClaims);
            if (!claimsResult.Succeeded)
            {
                var errorMessages = string.Join(", ", claimsResult.Errors.Select(e => e.Description));
                return BadRequest(errorMessages);
            }
            

            return Ok(new { message = "تم إنشاء المستخدم بنجاح!", userId = newUser.Id });
        }

        [Authorize]
        [HttpGet("current-user")]
        public async Task<ActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("المستخدم غير موجود");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok();

        }
    }
}
