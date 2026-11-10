namespace MeetingExtractor.Infrastructure.AiProviders;

public class AiSettings
{
    public const string SectionName = "AiSettings";

    public string Provider { get; set; } = "Ollama";
    public string GroqApiKey { get; set; } = string.Empty;
    public string OllamaBaseUrl { get; set; } = "http://localhost:11434/";
    public string OllamaModel { get; set; } = "llama3.2:3b";
}