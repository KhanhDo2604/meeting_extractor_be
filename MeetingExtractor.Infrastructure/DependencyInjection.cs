using MeetingExtractor.Application.Interfaces;
using MeetingExtractor.Infrastructure.AiProviders;
using MeetingExtractor.Infrastructure.Persistence;
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

        return services;
    }
}
