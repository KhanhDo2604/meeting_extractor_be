using MeetingExtractor.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace MeetingExtractor.Infrastructure.AiProviders;

public class GroqAiProvider: IAiProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public GroqAiProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<string> GenerateReplyAsync(string prompt)
    {
        var apiKey = _configuration["AiSettings:GroqApiKey"];
        var client = _httpClientFactory.CreateClient();

        var requestBody = new
        {
            model = "openai/gpt-oss-20b",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await client.PostAsync(
            "https://api.groq.com/openai/v1/chat/completions",
            jsonContent);

        var responseBody = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(responseBody);
        var messageContent = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return messageContent ?? string.Empty;
    }
}