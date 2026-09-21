using MeetingExtractor.Application.Interfaces;
using System.Text.Json;

namespace MeetingExtractor.Infrastructure.WhisperService;
public class WhisperHttpService : IWhisperService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WhisperHttpService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> TranscribeAsync(string audioFilePath)
    {
        var client = _httpClientFactory.CreateClient();

        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(audioFilePath);
        using var fileContent = new StreamContent(fileStream);

        form.Add(fileContent, "audio_file", Path.GetFileName(audioFilePath));

        var response = await client.PostAsync("http://localhost:8000/transcribe", form);
        var responseBody = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(responseBody);
        var transcript = document.RootElement.GetProperty("transcript").GetString();

        return transcript ?? string.Empty;
    }
}
