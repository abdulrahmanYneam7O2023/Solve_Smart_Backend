namespace Solve_Smart_Backend.Service
{
    public class AiEvaluationResult
    {
        /// <summary>
        /// هل الكود صحيح ويعطي النتيجة المتوقعة
        /// </summary>  
        public bool IsCorrect { get; set; }

        /// <summary>
        /// ملاحظات وتوجيهات من الذكاء الاصطناعي حول الكود
        /// </summary>
        public string Feedback { get; set; }

        /// <summary>
        /// الحل الصحيح الذي يقترحه الذكاء الاصطناعي (إذا كان الكود خاطئاً)
        /// </summary>
        public string CorrectSolution { get; set; }
        public double SuccessRate { get; set; }
    }
}
