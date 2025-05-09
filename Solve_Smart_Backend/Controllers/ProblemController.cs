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
    public class ProblemController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly IConfiguration _config;
        private readonly Solvedbcontext _context;

        public ProblemController(UserManager<Users> userManager, IConfiguration config, Solvedbcontext solvedbcontext)
        {
            _config = config;
            _userManager = userManager;
            _context = solvedbcontext;
        }

        [Authorize(Policy = "Manager")]
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
    }
}
