using AiChat.Api.Data;
using AiChat.Api.DTOs;
using AiChat.Api.Models;
using AiChat.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Api.Services.Implementations;

public sealed class ChatService(ApplicationDbContext dbContext, ILlmService llmService) : IChatService
{
    private const int HistoryLimit = 20;
    private const string SystemInstruction = "You are a helpful AI assistant. Give clear, accurate, and concise answers.";

    public async Task<ChatResponse> SendMessageAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        var content = request.Message.Trim();
        if (content.Length == 0)
        {
            throw new ArgumentException("Message cannot be empty.");
        }

        Conversation conversation;
        if (request.ConversationId is null)
        {
            var now = DateTime.UtcNow;
            conversation = new Conversation
            {
                Title = CreateTitle(content),
                CreatedAt = now,
                UpdatedAt = now
            };
            dbContext.Conversations.Add(conversation);
        }
        else
        {
            conversation = await dbContext.Conversations
                .SingleOrDefaultAsync(item => item.Id == request.ConversationId, cancellationToken)
                ?? throw new KeyNotFoundException("Conversation was not found.");
        }

        var previousMessages = request.ConversationId is null
            ? []
            : await dbContext.Messages
                .AsNoTracking()
                .Where(item => item.ConversationId == conversation.Id)
                .OrderByDescending(item => item.CreatedAt)
                .ThenByDescending(item => item.Id)
                .Take(HistoryLimit - 1)
                .OrderBy(item => item.CreatedAt)
                .ThenBy(item => item.Id)
                .Select(item => new LlmMessage(item.Role, item.Content))
                .ToListAsync(cancellationToken);

        var userMessage = new Message
        {
            Conversation = conversation,
            Role = "user",
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        previousMessages.Add(new LlmMessage("user", content));
        previousMessages.Insert(0, new LlmMessage("system", SystemInstruction));
        var llmResponse = await llmService.GetChatCompletionAsync(previousMessages, cancellationToken);

        var assistantMessage = new Message
        {
            Conversation = conversation,
            Role = "assistant",
            Content = llmResponse.Content,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Messages.AddRange(userMessage, assistantMessage);
        conversation.UpdatedAt = assistantMessage.CreatedAt;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ChatResponse(
            conversation.Id,
            new MessageDto(
                assistantMessage.Id,
                assistantMessage.Role,
                assistantMessage.Content,
                assistantMessage.CreatedAt));
    }

    public async Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Conversations
            .AsNoTracking()
            .OrderByDescending(item => item.UpdatedAt)
            .Select(item => new ConversationSummaryDto(item.Id, item.Title, item.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ConversationDetailsDto?> GetConversationAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await dbContext.Conversations
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new ConversationDetailsDto(
                item.Id,
                item.Title,
                item.CreatedAt,
                item.UpdatedAt,
                item.Messages
                    .OrderBy(message => message.CreatedAt)
                    .ThenBy(message => message.Id)
                    .Select(message => new MessageDto(
                        message.Id,
                        message.Role,
                        message.Content,
                        message.CreatedAt))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static string CreateTitle(string content)
    {
        const int maxLength = 60;
        return content.Length <= maxLength ? content : $"{content[..(maxLength - 1)]}…";
    }
}
