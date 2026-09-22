using MeetingExtractor.Application.Interfaces;
using MeetingExtractor.Application.Meetings;
using MeetingExtractor.Domain.Interfaces;
using MeetingExtractor.Infrastructure.AiProviders;
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
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpClient();
        services.AddScoped<IAiProvider, GroqAiProvider>();

        services.AddScoped<IWhisperService, WhisperHttpService>();

        services.AddScoped<IMeetingRepository, MeetingRepository>();

        services.AddScoped<IMeetingProcessingService,MeetingProcessingService>();
        return services;
    }
}
