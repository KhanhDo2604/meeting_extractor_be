using MeetingExtractor.Application.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MeetingExtractor.Infrastructure.AiProviders;

public class GroqAiProvider : IAiProvider
{
    public const string HttpClientName = "Groq";
    private const string Model = "openai/gpt-oss-20b";

    private readonly IHttpClientFactory _httpClientFactory;

    public GroqAiProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GenerateReplyAsync(string prompt)
    {
        var client = _httpClientFactory.CreateClient();

        var requestBody = new
        {
            model = Model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response = await client.PostAsJsonAsync("v1/chat/completions", requestBody);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);

        var messageContent = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return messageContent ?? string.Empty;
    }
}