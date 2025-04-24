using Solve_Smart_Backend.DDL.Models;
using System.ComponentModel.DataAnnotations;

namespace Solve_Smart_Backend.DTOs
{
    public class ProblemDto
    {
        
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string? Constraints { get; set; }

        [Required]
        public DifficultyLevel DifficultyLevel { get; set; }

    }
}
