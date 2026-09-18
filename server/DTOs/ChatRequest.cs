using System.ComponentModel.DataAnnotations;

namespace AiChat.Api.DTOs;

public sealed record ChatRequest(
    int? ConversationId,
    [Required, StringLength(8000, MinimumLength = 1)] string Message);
