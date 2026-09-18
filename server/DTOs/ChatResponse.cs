namespace AiChat.Api.DTOs;

public sealed record ChatResponse(int ConversationId, MessageDto Message);
