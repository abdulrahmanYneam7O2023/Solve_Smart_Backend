using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Solve_Smart_Backend.DDL.Context;
using Solve_Smart_Backend.Interface;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Solve_Smart_Backend.Service
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly Solvedbcontext _context;

        public AiService(HttpClient httpClient, IConfiguration configuration, Solvedbcontext context)
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

        public async Task<AiEvaluationResult> EvaluateCode(string problemDescription, string code, int languageId)
        {
            try
            {
                if (string.IsNullOrEmpty(problemDescription))
                {
                    throw new ArgumentNullException(nameof(problemDescription), "Problem description cannot be empty.");
                }
                if (string.IsNullOrEmpty(code))
                {
                    throw new ArgumentNullException(nameof(code), "Code cannot be empty.");
                }

                // تنظيف الكود المدخل
                code = code.Trim();
                if (code.StartsWith("\"") && code.EndsWith("\""))
                {
                    code = code.Substring(1, code.Length - 2);
                }
                code = code.Replace("\\\"", "\"");

                var language = await _context.languages.FindAsync(languageId);
                if (language == null)
                {
                    throw new Exception("لغة البرمجة غير موجودة");
                }

                var problem = await _context.problems
                    .FirstOrDefaultAsync(p => p.Description == problemDescription);
                if (problem == null)
                {
                    throw new Exception("المشكلة غير موجودة");
                }

                
                string languageName = languageId switch
                {
                    1 => "csharp",
                    2 => "java",
                    3 => "python",
                    4 => "cpp",
                    _ => language.Name.ToLower()
                };

                var prompt = $@"أنت مبرمج خبير ومقيم للكود. قم بتقييم هذا الكود وتحديد ما إذا كان يحل المشكلة المذكورة بشكل صحيح.

المشكلة:
{problemDescription}

القيود (إن وجدت):
{problem.Constraints ?? "لا توجد قيود محددة"}

حالات الاختبار:
المدخلات: {problem.TestCaseInput ?? "غير متوفر"}
المخرجات المتوقعة: {problem.TestCaseOutput ?? "غير متوفر"}

الكود المقدم (بلغة {language.Name}):
```{languageName}
{code}
```

قم بتقييم الكود وتقديم النتائج بالتنسيق التالي:
1. يبدأ بـ 'CORRECT' أو 'INCORRECT' بناءً على ما إذا كان الكود يحل المشكلة بشكل صحيح ويجتاز حالات الاختبار.
2. قدم تقييمًا عدديًا لمستوى صحة الحل (Success Rate) من 1 إلى 10 (يمكن استخدام أعداد كسرية)، حيث 10 تعني حل مثالي و1 تعني حل غير صحيح تمامًا.
3. إذا كان الكود صحيحًا (CORRECT)، اكتب فقط 'الكود صحيح' بدون تعليقات إضافية أو حل صحيح.
4. إذا كان الكود غير صحيح (INCORRECT)، قدم تعليقًا واضحاً ومختصر باللغة العربية يتضمن:
   - التقييم: شرح لماذا الكود غير صحيح.
   - نقاط القوة: أي جوانب إيجابية في الكود.
   - نقاط الضعف: المشاكل أو العيوب في الكود.
   - تحسينات محتملة: اقتراحات لتحسين الكود.
   - الحل الصحيح: قدم الحل الصحيح في نفس اللغة داخل code block (```).";

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

                Console.WriteLine($"Sending request to OpenAI: {JsonSerializer.Serialize(requestBody)}");

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

                return ParseAiResponse(aiMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EvaluateCode: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return new AiEvaluationResult
                {
                    IsCorrect = false,
                    Feedback = $"حدث خطأ أثناء تقييم الكود: {ex.Message}",
                    CorrectSolution = string.Empty,
                    SuccessRate = 1.0
                };
            }
        }

        private AiEvaluationResult ParseAiResponse(string aiMessage)
        {
            var result = new AiEvaluationResult();

            result.IsCorrect = aiMessage.StartsWith("CORRECT", StringComparison.OrdinalIgnoreCase);

            
            var successRateMatch = System.Text.RegularExpressions.Regex.Match(aiMessage, @"2\.\s*(\d+(\.\d+)?)");
            result.SuccessRate = successRateMatch.Success
                ? Math.Clamp(double.Parse(successRateMatch.Groups[1].Value), 1.0, 10.0)
                : (result.IsCorrect ? 10.0 : 1.0);

            if (result.IsCorrect)
            {
                result.Feedback = "الكود صحيح";
                result.CorrectSolution = string.Empty;
            }
            else
            {
                
                result.Feedback = aiMessage;

               
                var codeBlockStart = aiMessage.LastIndexOf("```");
                var codeBlockEnd = aiMessage.LastIndexOf("```", codeBlockStart - 1);
                if (codeBlockStart > codeBlockEnd && codeBlockEnd >= 0)
                {
                    result.CorrectSolution = aiMessage
                        .Substring(codeBlockEnd + 3, codeBlockStart - codeBlockEnd - 3)
                        .Trim();
                }
                else
                {
                   
                    var solutionStart = aiMessage.IndexOf("- الحل الصحيح:");
                    if (solutionStart >= 0)
                    {
                        result.CorrectSolution = aiMessage.Substring(solutionStart + 14).Trim();
                    }
                    else
                    {
                        result.CorrectSolution = string.Empty;
                    }
                }

                if (!string.IsNullOrEmpty(result.CorrectSolution))
                {
                    var solutionIndex = result.Feedback.IndexOf(result.CorrectSolution);
                    if (solutionIndex >= 0)
                    {
                        result.Feedback = result.Feedback.Substring(0, solutionIndex).Trim();
                    }
                }
            }

            return result;
        }
    }

    public class OpenAIResponse
    {
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Content { get; set; }
    }
}
