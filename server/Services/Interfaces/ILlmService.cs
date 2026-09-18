namespace AiChat.Api.Services.Interfaces;

public interface ILlmService
{
    Task<LlmResponse> GetChatCompletionAsync(
        IEnumerable<LlmMessage> messages,
        CancellationToken cancellationToken = default);
}

public sealed record LlmMessage(string Role, string Content);
public sealed record LlmResponse(string Content);
