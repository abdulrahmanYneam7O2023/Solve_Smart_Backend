using Solve_Smart_Backend.DTOs;
using Solve_Smart_Backend.Service;

namespace Solve_Smart_Backend.Interface
{
    public interface IAiServiceProblems
    {
       
           
             Task<ProblemDto> GenerateProblem(string problemDescription); // الدالة الجديدة
        
    }
}
