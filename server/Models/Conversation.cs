namespace AiChat.Api.Models;

public sealed class Conversation
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Message> Messages { get; set; } = [];
}
