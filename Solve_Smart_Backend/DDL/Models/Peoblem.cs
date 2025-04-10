using Solve_Smart_Backend.DDL.Models;
using System.ComponentModel.DataAnnotations;

public class Problem
{
    [Key]
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

    public int bestsolution { get; set; }
    public Best_Solution best_Solution { get; set; }

    public ICollection<TestCases> testCases { get; set; }
    public ICollection<UserProblem> UserProblems { get; set; }
}