namespace AiChat.Api.DTOs;

public sealed record ConversationSummaryDto(int Id, string Title, DateTime UpdatedAt);

public sealed record ConversationDetailsDto(
    int Id,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<MessageDto> Messages);
