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
        [HttpPost("addProblem")]
        public async Task<IActionResult> registr([FromBody] ProblemDto problemDto)
        {
            var DbEmp = new Problem()
            {
                Title = problemDto.Title,
                Description = problemDto.Description,
                Constraints = problemDto.Constraints,
                DifficultyLevel = problemDto.DifficultyLevel
            };
            await _context.problems.AddAsync(DbEmp);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet("getProblems")]
        public async Task<IActionResult> getProblems()
        {
            var problems = await _context.problems.ToListAsync();
            return Ok(problems);
        }
        [HttpGet("getProblem/{id}")]
        public async Task<IActionResult> getProblem(int id)
        {
            var problem = await _context.problems.FindAsync(id);
            if (problem == null)
            {
                return NotFound("cant found this Problem");
            }
            return Ok(problem);
        }
        [HttpPut("updateProblem/{id}")]
        public async Task<IActionResult> updateProblem(int id, [FromBody] ProblemDto problemDto)
        {
            var problem = await _context.problems.FindAsync(id);
            if (problem == null)
            {
                return NotFound("cant found this Problem");
            }
            problem.Title = problemDto.Title;
            problem.Description = problemDto.Description;
            problem.Constraints = problemDto.Constraints;
            problem.DifficultyLevel = problemDto.DifficultyLevel;

            _context.problems.Update(problem);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("deleteProblem/{id}")]
        public async Task<IActionResult> deleteProblem(int id)
        {
            var problem = await _context.problems.FindAsync(id);
            if (problem == null)
            {
                return NotFound("cant found this Problem");
            }
            _context.problems.Remove(problem);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
