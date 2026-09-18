using AiChat.Api.Configuration;
using AiChat.Api.Data;
using AiChat.Api.Middleware;
using AiChat.Api.Services.Implementations;
using AiChat.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services.AddOptions<OpenRouterOptions>()
    .Configure(options =>
    {
        builder.Configuration.GetSection(OpenRouterOptions.SectionName).Bind(options);
        options.ApiKey = builder.Configuration["OPENROUTER_API_KEY"] ?? options.ApiKey;
    })
    .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "OpenRouter BaseUrl must be an absolute URL.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Model), "OpenRouter Model is required.")
    .ValidateOnStart();

builder.Services.AddHttpClient<ILlmService, OpenRouterLlmService>((services, client) =>
{
    var options = services.GetRequiredService<IOptions<OpenRouterOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(90);
});

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevelopment", policy =>
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("ReactDevelopment");
}

app.MapControllers();
app.Run();

public partial class Program;
