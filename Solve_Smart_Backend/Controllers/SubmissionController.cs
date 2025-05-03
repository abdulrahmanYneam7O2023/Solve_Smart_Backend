using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Solve_Smart_Backend.DDL.Context;
using Solve_Smart_Backend.DDL.Models;
using Solve_Smart_Backend.DTOs;
using Solve_Smart_Backend.Interface;
using Solve_Smart_Backend.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Solve_Smart_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionController : ControllerBase
    {
        private readonly Solvedbcontext _context;
        private readonly IAiService _aiService;
        private readonly UserManager<Users> _userManager;
        private readonly IConfiguration _configuration;

        public SubmissionController(Solvedbcontext context, IAiService aiService, UserManager<Users>  userManager, IConfiguration configuration)
        {
            _context = context;
            _aiService = aiService;
            _userManager = userManager;
            _configuration = configuration;
        }

      

        private string GenerateJwtToken(IdentityUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("SubmitCode")]
        public async Task<IActionResult> SubmitCode([FromBody] SubmissionDto submissionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                string userId = !string.IsNullOrEmpty(submissionDto.UserId) ? submissionDto.UserId : currentUserId;

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest(new { message = "المستخدم غير موجود" });
                }

                var problem = await _context.problems.FindAsync(submissionDto.ProblemId);
                if (problem == null)
                {
                    return BadRequest(new { message = "المشكلة غير موجودة" });
                }

                var language = await _context.languages.FindAsync(submissionDto.LanguageId);
                if (language == null)
                {
                    return BadRequest(new { message = "لغة البرمجة غير موجودة" });
                }

                var userProblem = await _context.Users_Problems.FirstOrDefaultAsync(
                    up => up.UserId == userId && up.ProblemId == submissionDto.ProblemId);

                AiEvaluationResult aiResult = null;
                try
                {
                    aiResult = await _aiService.EvaluateCode(
                        problemDescription: problem.Description,
                        code: submissionDto.Code,
                        languageId: submissionDto.LanguageId
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AI evaluation failed: {ex.Message}");
                    aiResult = new AiEvaluationResult
                    {
                        IsCorrect = false,
                        Feedback = $"فشل تقييم الكود بواسطة الذكاء الاصطناعي: {ex.Message}",
                        CorrectSolution = null,
                        SuccessRate = 1.0
                    };
                }

                if (userProblem == null)
                {
                    userProblem = new UserProblem
                    {
                        UserId = userId,
                        ProblemId = submissionDto.ProblemId,
                        TriesCount = 1,
                        IsSolved = aiResult.IsCorrect
                    };
                    await _context.Users_Problems.AddAsync(userProblem);
                }
                else
                {
                    userProblem.TriesCount++;
                    if (aiResult.IsCorrect)
                    {
                        userProblem.IsSolved = true;
                    }
                    _context.Users_Problems.Update(userProblem);
                }
                await _context.SaveChangesAsync();

                var submission = new Submission
                {
                    isSuccesfull = aiResult.IsCorrect,
                    Code = submissionDto.Code,
                    SubmissionTime = DateTime.UtcNow,
                    LanguagesId = submissionDto.LanguageId,
                    UserId = userId,
                    ProblemId = submissionDto.ProblemId,
                    UserProblemId = userProblem.Id,
                    SuccessRate = aiResult.SuccessRate,
                    AiFeedbackId = null
                };

                await _context.submissions.AddAsync(submission);
                await _context.SaveChangesAsync();

                if (aiResult != null)
                {
                    var aiFeedback = new Ai_Feedback
                    {
                        SubmissionId = submission.SId,
                        IsCorrect = aiResult.IsCorrect,
                        Feedback = aiResult.Feedback,
                        CorrectSolution = aiResult.CorrectSolution
                    };
                    await _context.ai_feedbacks.AddAsync(aiFeedback);
                    await _context.SaveChangesAsync();

                    submission.AiFeedbackId = aiFeedback.Id;
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = aiResult.IsCorrect ? "تم تقديم الحل بنجاح" : "محاولة غير ناجحة، حاول مرة أخرى",
                    submissionId = submission.SId,
                    aiEvaluation = new
                    {
                        isCorrect = aiResult.IsCorrect,
                        successRate = aiResult.SuccessRate,
                        feedback = aiResult.Feedback,
                        correctSolution = aiResult.CorrectSolution
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SubmitCode: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء حفظ التقديم",
                    error = ex.Message + (ex.InnerException != null ? $"; Inner: {ex.InnerException.Message}" : "")
                });
            }
        }

        [HttpGet("GetUserSubmissions")]
        public async Task<IActionResult> GetUserSubmissions()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { message = "معرف المستخدم غير متوفر" });
                }

                var submissions = await _context.submissions
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.SubmissionTime)
                    .Include(s => s.userProblem)
                    .ThenInclude(up => up.Problem)
                    .Include(s => s.Languages)
                    .Include(s => s.AiFeedback)
                    .Select(s => new
                    {
                        s.SId,
                        s.isSuccesfull,
                        s.SubmissionTime,
                        s.ProblemId,
                        ProblemTitle = s.userProblem != null && s.userProblem.Problem != null ? s.userProblem.Problem.Title : null,
                        Language = s.Languages != null ? s.Languages.Name : null,
                        s.SuccessRate,
                        AiEvaluation = s.AiFeedback != null ? new
                        {
                            IsCorrect = s.AiFeedback.IsCorrect,
                            Feedback = s.AiFeedback.Feedback,
                            CorrectSolution = s.AiFeedback.CorrectSolution
                        } : null
                    })
                    .ToListAsync();

                return Ok(submissions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserSubmissions: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء استرجاع التقديمات",
                    error = ex.Message + (ex.InnerException != null ? $"; Inner: {ex.InnerException.Message}" : "")
                });
            }
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
    