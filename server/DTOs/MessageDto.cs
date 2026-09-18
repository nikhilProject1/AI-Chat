namespace AiChat.Api.DTOs;

public sealed record MessageDto(int Id, string Role, string Content, DateTime CreatedAt);
