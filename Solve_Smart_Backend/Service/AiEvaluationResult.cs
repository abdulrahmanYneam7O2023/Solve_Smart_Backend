namespace Solve_Smart_Backend.Service
{
    public class AiEvaluationResult
    {
       
        public bool IsCorrect { get; set; }
        public string Feedback { get; set; }
        public string CorrectSolution { get; set; }
        public double SuccessRate { get; set; }
    }
}
