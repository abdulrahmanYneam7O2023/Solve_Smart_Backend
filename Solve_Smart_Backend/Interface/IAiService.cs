using Solve_Smart_Backend.Service;

namespace Solve_Smart_Backend.Interface
{
    public interface IAiService
    {
        /// <summary>
        /// rate
        /// </summary>
        /// <param name="problemDescription">des</param>
        /// <param name="code">answer</param>
        /// <param name="languageId">programing language</param>
        /// <returns>result</returns>
        Task<AiEvaluationResult> EvaluateCode(string problemDescription, string code, int languageId);
    }
}
