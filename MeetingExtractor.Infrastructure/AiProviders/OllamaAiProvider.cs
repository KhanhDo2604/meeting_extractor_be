using System.Net.Http.Json;
using System.Text.Json;
using MeetingExtractor.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace MeetingExtractor.Infrastructure.AiProviders;

public class OllamaAiProvider : IAiProvider
{
    public const string HttpClientName = "Ollama";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AiSettings _aiSettings;

    public OllamaAiProvider(IHttpClientFactory httpClientFactory, IOptions<AiSettings> aiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _aiSettings = aiSettings.Value;
    }

    public async Task<string> GenerateReplyAsync(string prompt)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

        var requestBody = new
        {
            model = _aiSettings.OllamaModel,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            response_format = new { type = "json_object" }
        };

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