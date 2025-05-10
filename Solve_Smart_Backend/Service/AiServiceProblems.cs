using Solve_Smart_Backend.DDL.Models;
using Solve_Smart_Backend.DTOs;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Solve_Smart_Backend.DDL.Context;
using System.Net.Http.Headers;
using Solve_Smart_Backend.Interface;

namespace Solve_Smart_Backend.Service
{
    public class AiServiceProblems : IAiServiceProblems
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly Solvedbcontext _context;

        public AiServiceProblems(HttpClient httpClient, IConfiguration configuration, Solvedbcontext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;

            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException("OpenAI API key is not configured.");
            }

            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            Console.WriteLine("OpenAI API configured successfully.");
        }
        public async Task<ProblemDto> GenerateProblem(string problemDescription)
        {
            try
            {
                if (string.IsNullOrEmpty(problemDescription))
                {
                    throw new ArgumentNullException(nameof(problemDescription), "وصف المشكلة لا يمكن أن يكون فارغًا.");
                }

                // بناء الـ prompt للـ AI
                var prompt = $@"أنت خبير في تصميم مسائل البرمجة. بناءً على الوصف التالي، قم بإنشاء مشكلة برمجة كاملة تحتوي على العناصر التالية:
الوصف:
{problemDescription}

يرجى تقديم النتائج بالتنسيق التالي (كل جزء منفصل وواضح):
1. العنوان (Title): عنوان موجز وواضح للمشكلة (بحد أقصى 100 حرف).
2. الوصف (Description): وصف تفصيلي للمشكلة (بحد أقصى 1000 حرف).
3. القيود (Constraints): أي قيود على الحل (مثل حدود المدخلات، أو الوقت/الذاكرة، إن وجدت).
4. مستوى الصعوبة (DifficultyLevel): حدد المستوى (سهل، متوسط، صعب) باستخدام الكلمات: Easy، Medium، Hard.
5. حالات الاختبار - المدخلات (TestCaseInput): مثال على المدخلات (بحد أقصى 500 حرف).
6. حالات الاختبار - المخرجات (TestCaseOutput): المخرجات المتوقعة بناءً على المدخلات (بحد أقصى 500 حرف).
7. الحل الأمثل (Best_Solution): قدم الحل الأمثل في لغة C# داخل code block (```csharp\nالحل\n```).

يرجى تقديم كل جزء في سطر منفصل مع التنسيق المطلوب.";

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                new { role = "user", content = prompt }
            },
                    temperature = 0.1,
                    max_tokens = 2048
                };

                Console.WriteLine($"Sending request to OpenAI for problem generation: {JsonSerializer.Serialize(requestBody)}");

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("chat/completions", content);

                Console.WriteLine($"OpenAI response status: {response.StatusCode}");
                Console.WriteLine($"OpenAI response headers: {response.Headers.ToString()}");

                var responseString = await response.Content.ReadAsStringAsync();
                var logResponse = responseString.Length > 1000 ? responseString.Substring(0, 1000) + "..." : responseString;
                Console.WriteLine($"OpenAI response content: {logResponse}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"OpenAI error content: {responseString}");
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        throw new Exception("فشل طلب OpenAI: تم تجاوز حد الطلبات. حاول لاحقًا.");
                    }
                    throw new Exception($"فشل طلب OpenAI: {response.StatusCode}, {responseString}");
                }

                var openAiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var aiMessage = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content;
                if (string.IsNullOrEmpty(aiMessage))
                {
                    throw new Exception("لم يتم استلام إجابة من OpenAI. الاستجابة: " + responseString);
                }

                return ParseProblemResponse(aiMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateProblem: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                throw new Exception($"حدث خطأ أثناء إنشاء المشكلة: {ex.Message}");
            }
        }

        private ProblemDto ParseProblemResponse(string aiMessage)
        {
            var problem = new ProblemDto();

            // تقسيم الـ response بناءً على السطور
            var lines = aiMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.StartsWith("1. العنوان (Title):"))
                {
                    problem.Title = line.Replace("1. العنوان (Title):", "").Trim();
                }
                else if (line.StartsWith("2. الوصف (Description):"))
                {
                    problem.Description = line.Replace("2. الوصف (Description):", "").Trim();
                }
                else if (line.StartsWith("3. القيود (Constraints):"))
                {
                    problem.Constraints = line.Replace("3. القيود (Constraints):", "").Trim();
                    if (string.IsNullOrEmpty(problem.Constraints) || problem.Constraints == "لا توجد قيود")
                    {
                        problem.Constraints = null;
                    }
                }
                else if (line.StartsWith("4. مستوى الصعوبة (DifficultyLevel):"))
                {
                    var level = line.Replace("4. مستوى الصعوبة (DifficultyLevel):", "").Trim();
                    problem.DifficultyLevel = level switch
                    {
                        "Easy" => DifficultyLevel.Easy,
                        "Medium" => DifficultyLevel.Medium,
                        "Hard" => DifficultyLevel.Hard,
                        _ => DifficultyLevel.Medium // Default
                    };
                }
                else if (line.StartsWith("5. حالات الاختبار - المدخلات (TestCaseInput):"))
                {
                    problem.TestCaseInput = line.Replace("5. حالات الاختبار - المدخلات (TestCaseInput):", "").Trim();
                }
                else if (line.StartsWith("6. حالات الاختبار - المخرجات (TestCaseOutput):"))
                {
                    problem.TestCaseOutput = line.Replace("6. حالات الاختبار - المخرجات (TestCaseOutput):", "").Trim();
                }
                else if (line.StartsWith("```csharp"))
                {
                    // استخراج الحل الأمثل من الـ code block
                    var solutionStart = aiMessage.IndexOf("```csharp") + 9;
                    var solutionEnd = aiMessage.IndexOf("```", solutionStart);
                    if (solutionEnd > solutionStart)
                    {
                        problem.Best_Solution = aiMessage.Substring(solutionStart, solutionEnd - solutionStart).Trim();
                    }
                }
            }

            // التحقق من الحقول المطلوبة
            if (string.IsNullOrEmpty(problem.Title))
                problem.Title = "Untitled Problem";
            if (string.IsNullOrEmpty(problem.Description))
                problem.Description = "No description provided.";
            if (string.IsNullOrEmpty(problem.TestCaseInput))
                problem.TestCaseInput = "No test case input provided.";
            if (string.IsNullOrEmpty(problem.TestCaseOutput))
                problem.TestCaseOutput = "No test case output provided.";
            if (string.IsNullOrEmpty(problem.Best_Solution))
                problem.Best_Solution = "// No solution provided.";

            return problem;
        }
    }
}
