namespace AiChat.Api.Configuration;

public sealed class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
    public string Model { get; set; } = "openrouter/free";
    public string ApplicationName { get; set; } = "AI Chat Learning Project";
    public string? ApiKey { get; set; }
}
