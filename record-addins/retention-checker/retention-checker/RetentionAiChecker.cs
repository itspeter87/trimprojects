using OpenAI;
using OpenAI.Chat;
using System.Data;
using System.Threading.Tasks;

namespace retention_checker
{
    public class RetentionAiChecker
    {
        private readonly OpenAIClient _client;

        public RetentionAiChecker(string apiKey)
        {
            _client = new OpenAIClient(apiKey);
        }

        public async Task<string> CheckRetentionAsync(string documentText, string appliedRetention)
        {
            string systemPrompt = @"
You are an expert in Victorian Public Records Office (PROV) retention and disposal.
Your job is to classify documents according to PROS 07/01 and confirm whether the
applied retention schedule is correct.

Return your answer in this exact format:

Correct: Yes/No
Correct class: [PROS 07/01 class code and title]
Explanation: [short explanation]
";

            string userPrompt = $@"
Document content:
-----------------
{documentText}

Applied retention: {appliedRetention}

Determine:
1. The correct PROS 07/01 retention class.
2. Whether the applied retention is correct.
3. Provide a short explanation.
";

            var chat = _client.GetChatClient("gpt-4o");

            var response = await chat.Completions.CreateAsync(
                new[]
                {
                    new ChatMessage(Role.System, systemPrompt),
                    new ChatMessage(Role.User, userPrompt)
                });

            return response.Content[0].Text;
        }
    }
}
