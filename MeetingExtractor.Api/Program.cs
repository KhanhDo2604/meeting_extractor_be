using Hangfire;
using Hangfire.PostgreSql;
using MeetingExtractor.Infrastructure;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Path = System.IO.Path;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options => 
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services
    .AddGraphQLServer()
    .AddQueryType<MeetingExtractor.Api.GraphQL.Query>()
    .AddMutationType<MeetingExtractor.Api.GraphQL.Mutation>()
    .AddSubscriptionType<MeetingExtractor.Api.GraphQL.Subscription>()
    .AddInMemorySubscriptions();

var app = builder.Build();

app.MapGraphQL();

if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "UploadedAudios");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
