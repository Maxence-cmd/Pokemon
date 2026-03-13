using Pokemon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pokemon.Services
{
    public class GitHubAiService
    {
        private readonly HttpClient _http;
        private readonly string _token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        public GitHubAiService()
        {
            _http = new HttpClient();
            _http.BaseAddress = new Uri("https://models.inference.ai.azure.com/");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            _http.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<string> ChatAsync(List<ChatMessage> messages)
        {
            var payload = new
            {
                model = "gpt-4o-mini",
                messages = messages,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(payload);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync("chat/completions", content);
            var result = await response.Content.ReadAsStringAsync();


            using var doc = JsonDocument.Parse(result);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
    }
}
