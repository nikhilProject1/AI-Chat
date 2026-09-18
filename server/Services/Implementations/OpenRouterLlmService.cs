using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiChat.Api.Configuration;
using AiChat.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AiChat.Api.Services.Implementations;

public sealed class OpenRouterLlmService(
    HttpClient httpClient,
    IOptions<OpenRouterOptions> options,
    ILogger<OpenRouterLlmService> logger) : ILlmService
{
    public async Task<LlmResponse> GetChatCompletionAsync(
        IEnumerable<LlmMessage> messages,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new LlmServiceException(
                "OpenRouter is not configured. Add OpenRouter:ApiKey to .NET User Secrets or set OPENROUTER_API_KEY.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        request.Headers.TryAddWithoutValidation("X-Title", settings.ApplicationName);
        request.Content = JsonContent.Create(new OpenRouterRequest(settings.Model, messages));

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("OpenRouter returned HTTP {StatusCode}.", (int)response.StatusCode);
                throw new LlmServiceException(
                    $"The AI provider returned an error ({(int)response.StatusCode}). Please try again.");
            }

            var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>(cancellationToken);
            var content = result?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new LlmServiceException("The AI provider returned an empty response.");
            }

            return new LlmResponse(content.Trim());
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("OpenRouter request timed out.");
            throw new LlmServiceException("The AI provider took too long to respond. Please try again.");
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "OpenRouter request failed.");
            throw new LlmServiceException("The AI provider could not be reached. Please try again.");
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "OpenRouter returned an invalid response.");
            throw new LlmServiceException("The AI provider returned an invalid response. Please try again.");
        }
    }

    private sealed record OpenRouterRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IEnumerable<LlmMessage> Messages);

    private sealed record OpenRouterResponse(
        [property: JsonPropertyName("choices")] IReadOnlyList<OpenRouterChoice>? Choices);

    private sealed record OpenRouterChoice(
        [property: JsonPropertyName("message")] OpenRouterMessage? Message);

    private sealed record OpenRouterMessage(
        [property: JsonPropertyName("content")] string Content);
}

public sealed class LlmServiceException(string message) : Exception(message);
