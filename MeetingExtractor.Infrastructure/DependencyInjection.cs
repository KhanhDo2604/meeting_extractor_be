using System.Net.Http.Headers;
using MeetingExtractor.Application.Interfaces;
using MeetingExtractor.Application.Meetings;
using MeetingExtractor.Domain.Interfaces;
using MeetingExtractor.Infrastructure.AiProviders;
using MeetingExtractor.Infrastructure.Events;
using MeetingExtractor.Infrastructure.Persistence;
using MeetingExtractor.Infrastructure.Persistence.Repositories;
using MeetingExtractor.Infrastructure.WhisperService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingExtractor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var aiSettings = configuration.GetSection(AiSettings.SectionName).Get<AiSettings>() ?? new AiSettings();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. Set it with dotnet user-secrets.");
        }

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        services.AddHttpClient();

        switch (aiSettings.Provider)
        {
            case var provider when string.Equals(provider, "Groq", StringComparison.OrdinalIgnoreCase):
                if (string.IsNullOrWhiteSpace(aiSettings.GroqApiKey))
                {
                    throw new InvalidOperationException(
                        "AiSettings:GroqApiKey is not configured. Set it with dotnet user-secrets.");
                }

                services.AddHttpClient(GroqAiProvider.HttpClientName, client =>
                {
                    client.BaseAddress = new Uri("https://api.groq.com/openai/");
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aiSettings.GroqApiKey);
                });
                services.AddScoped<IAiProvider, GroqAiProvider>();
                break;

            case var provider when string.Equals(provider, "Ollama", StringComparison.OrdinalIgnoreCase):
                services.AddHttpClient(OllamaAiProvider.HttpClientName, client =>
                {
                    client.BaseAddress = new Uri(aiSettings.OllamaBaseUrl);
                    client.Timeout = TimeSpan.FromMinutes(5);
                });
                services.AddScoped<IAiProvider, OllamaAiProvider>();
                break;

            default:
                throw new InvalidOperationException($"Unsupported AI provider '{aiSettings.Provider}'.");
        }


        services.AddScoped<IWhisperService, WhisperHttpService>();

        services.AddScoped<IMeetingRepository, MeetingRepository>();

        services.AddScoped<IMeetingProcessingService, MeetingProcessingService>();

        services.AddScoped<IMeetingEventPublisher, MeetingEventPublisher>();
        return services;
    }
}
