using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solve_Smart_Backend.DDL.Context;
using Solve_Smart_Backend.DDL.Models;
using Solve_Smart_Backend.DTOs;
using Solve_Smart_Backend.Interface;

namespace Solve_Smart_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProblemController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly IConfiguration _config;
        private readonly Solvedbcontext _context;
        private readonly IAiServiceProblems _aiService;

        public ProblemController(UserManager<Users> userManager, IConfiguration config, Solvedbcontext solvedbcontext , IAiServiceProblems aiService)
        {
            _config = config;
            _userManager = userManager;
            _context = solvedbcontext;
            _aiService = aiService;
        }

        //[Authorize(Policy = "Manager")]
        [HttpPost("addProblem")]
        public async Task<IActionResult> AddProblem([FromBody] ProblemDto problemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var DbEmp = new Problem()
            {
                Title = problemDto.Title,
                Description = problemDto.Description,
                Constraints = problemDto.Constraints,
                DifficultyLevel = problemDto.DifficultyLevel,
                TestCaseInput = problemDto.TestCaseInput,
                TestCaseOutput = problemDto.TestCaseOutput,
                Best_Solution = problemDto.Best_Solution
            };

            await _context.problems.AddAsync(DbEmp);
            await _context.SaveChangesAsync();

            return Ok(new { id = DbEmp.Id, message = "تمت إضافة المشكلة بنجاح" });
        }

        [Authorize]
        [HttpGet("getProblems")]
        public async Task<IActionResult> GetProblems()
        {
            var problems = await _context.problems.ToListAsync();
            return Ok(problems);
        }

        [Authorize]
        [HttpGet("getProblem/{id}")]
        public async Task<IActionResult> GetProblem(int id)
        {
            var problem = await _context.problems.FindAsync(id);
            if (problem == null)
            {
                return NotFound("لم يتم العثور على هذه المشكلة");
            }
            return Ok(problem);
        }
        //[Authorize(Policy = "Manager")]
        [HttpPut("updateProblem/{id}")]
        public async Task<IActionResult> UpdateProblem(int id, [FromBody] ProblemDto problemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var problem = await _context.problems.FindAsync(id);
            if (problem == null)
            {
                return NotFound("لم يتم العثور على هذه المشكلة");
            }

            problem.Title = problemDto.Title;
            problem.Description = problemDto.Description;
            problem.Constraints = problemDto.Constraints;
            problem.DifficultyLevel = problemDto.DifficultyLevel;
            problem.TestCaseInput = problemDto.TestCaseInput;
            problem.TestCaseOutput = problemDto.TestCaseOutput;
            problem.Best_Solution = problemDto.Best_Solution;

            _context.problems.Update(problem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تحديث المشكلة بنجاح" });
        }
        [Authorize(Policy = "Manager")]
        [HttpDelete("deleteProblem/{id}")]
        public async Task<IActionResult> DeleteProblem(int id)
        {
            var problem = await _context.problems.FindAsync(id);
            if (problem == null)
            {
                return NotFound("لم يتم العثور على هذه المشكلة");
            }

            _context.problems.Remove(problem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف المشكلة بنجاح" });
        }
        //[Authorize]
        [HttpPost("GenerateProblem")]
        public async Task<IActionResult> GenerateProblem([FromBody] ProblemDescriptionDto descriptionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // التحقق من إن المستخدم أدمن
                var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                var isAdmin = userId == "007a8b7e-b8fb-47c3-a2ec-42c718604b03"; // استبدل بالـ ID الحقيقي بتاع الأدمن
                if (!isAdmin)
                {
                    return Unauthorized(new { message = "غير مصرح لك بإنشاء مشكلة. هذه الخاصية للأدمن فقط." });
                }

                // استدعاء الـ AI لإنشاء المشكلة
                var problemDto = await _aiService.GenerateProblem(descriptionDto.Description);

                // تحويل الـ ProblemDto لـ Problem وإضافته للـ database
                var problem = new Problem
                {
                    Title = problemDto.Title,
                    Description = problemDto.Description,
                    Constraints = problemDto.Constraints,
                    DifficultyLevel = problemDto.DifficultyLevel,
                    TestCaseInput = problemDto.TestCaseInput,
                    TestCaseOutput = problemDto.TestCaseOutput,
                    Best_Solution = problemDto.Best_Solution
                };

                await _context.problems.AddAsync(problem);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "تم إنشاء المشكلة بنجاح",
                    problemId = problem.Id,
                    problem = problemDto
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateProblem: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء إنشاء المشكلة",
                    error = ex.Message + (ex.InnerException != null ? $"; Inner: {ex.InnerException.Message}" : "")
                });
            }
        }
        [Authorize]
        [HttpPut("UpdateProblemWithAI/{id}")]
        public async Task<IActionResult> UpdateProblemWithAI(int id, [FromBody] ProblemDescriptionDto descriptionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // التحقق من إن المستخدم أدمن
                var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                var isAdmin = userId == "007a8b7e-b8fb-47c3-a2ec-42c718604b03"; // الـ ID بتاع الأدمن
                if (!isAdmin)
                {
                    return Unauthorized(new { message = "غير مصرح لك بتعديل المشكلة. هذه الخاصية للأدمن فقط." });
                }

                // التحقق من وجود المشكلة
                var existingProblem = await _context.problems.FindAsync(id);
                if (existingProblem == null)
                {
                    return NotFound(new { message = "لم يتم العثور على هذه المشكلة" });
                }

                // استدعاء الـ AI لإنشاء المشكلة المعدلة
                var updatedProblemDto = await _aiService.GenerateProblem(descriptionDto.Description);

                // تحديث بيانات المشكلة
                existingProblem.Title = updatedProblemDto.Title;
                existingProblem.Description = updatedProblemDto.Description;
                existingProblem.Constraints = updatedProblemDto.Constraints;
                existingProblem.DifficultyLevel = updatedProblemDto.DifficultyLevel;
                existingProblem.TestCaseInput = updatedProblemDto.TestCaseInput;
                existingProblem.TestCaseOutput = updatedProblemDto.TestCaseOutput;
                existingProblem.Best_Solution = updatedProblemDto.Best_Solution;

                _context.problems.Update(existingProblem);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "تم تحديث المشكلة بنجاح",
                    problemId = existingProblem.Id,
                    problem = updatedProblemDto
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateProblemWithAI: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء تحديث المشكلة",
                    error = ex.Message + (ex.InnerException != null ? $"; Inner: {ex.InnerException.Message}" : "")
                });
            }
        }
        [Authorize]
        [HttpDelete("DeleteProblemWithAI/{id}")]
        public async Task<IActionResult> DeleteProblemWithAI(int id)
        {
            try
            {
                // التحقق من إن المستخدم أدمن
                var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                var isAdmin = userId == "007a8b7e-b8fb-47c3-a2ec-42c718604b03"; // الـ ID بتاع الأدمن
                if (!isAdmin)
                {
                    return Unauthorized(new { message = "غير مصرح لك بحذف المشكلة. هذه الخاصية للأدمن فقط." });
                }

                // التحقق من وجود المشكلة
                var problem = await _context.problems.FindAsync(id);
                if (problem == null)
                {
                    return NotFound(new { message = "لم يتم العثور على هذه المشكلة" });
                }

                _context.problems.Remove(problem);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "تم حذف المشكلة بنجاح",
                    problemId = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteProblemWithAI: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء حذف المشكلة",
                    error = ex.Message + (ex.InnerException != null ? $"; Inner: {ex.InnerException.Message}" : "")
                });
            }
        }
    }

    public class ProblemDescriptionDto
    {
        [Required(ErrorMessage = "وصف المشكلة مطلوب")]
        public string Description { get; set; }
    }
}

