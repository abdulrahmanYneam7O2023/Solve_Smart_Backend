using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solve_Smart_Backend.DDL.Context;
using Solve_Smart_Backend.DDL.Models;
using Solve_Smart_Backend.DTOs;

namespace Solve_Smart_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguagesController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly Solvedbcontext _context;
        public LanguagesController(IConfiguration config, Solvedbcontext solvedbcontext)
        {
            _config = config;
            _context = solvedbcontext;
        }
        [Authorize(Policy = "Manager")]
        [HttpPost("addLanguage")]
        public async Task<IActionResult> AddLanguage([FromBody] LanguagesDto languagesDto)
        {
            var DbEmp = new Languages()
            {
                Name = languagesDto.Name
            };
            await _context.languages.AddAsync(DbEmp);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [Authorize]
        [HttpGet("getLanguages")]
        public async Task<IActionResult> GetLanguages()
        {
            var languages = await _context.languages.ToListAsync();
            return Ok(languages);
        }
      
        [HttpGet("getLanguage/{id}")]
        public async Task<IActionResult> GetLanguage(int id)
        {
            var language = await _context.languages.FindAsync(id);
            if (language == null)
            {
                return NotFound("cant found this language");
            }
            return Ok(language);
        }
        [Authorize(Policy = "Manager")]
        [HttpPut("updateLanguage/{id}")]
        public async Task<IActionResult> UpdateLanguage(int id, [FromBody] LanguagesDto languagesDto)
        {
            var language = await _context.languages.FindAsync(id);
            if (language == null)
            {
                return NotFound("cant found this language");
            }
            language.Name = languagesDto.Name;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [Authorize(Policy = "Manager")]
        [HttpDelete("deleteLanguage/{id}")]
        public async Task<IActionResult> DeleteLanguage(int id)
        {
            var language = await _context.languages.FindAsync(id);
            if (language == null)
            {
                return NotFound("cant found this language");
            }
            _context.languages.Remove(language);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
