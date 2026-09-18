using AiChat.Api.DTOs;

namespace AiChat.Api.Services.Interfaces;

public interface IChatService
{
    Task<ChatResponse> SendMessageAsync(ChatRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsAsync(CancellationToken cancellationToken);
    Task<ConversationDetailsDto?> GetConversationAsync(int id, CancellationToken cancellationToken);
}
