using AiChat.Api.DTOs;
using AiChat.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ConversationsController(IChatService chatService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationSummaryDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await chatService.GetConversationsAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConversationDetailsDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var conversation = await chatService.GetConversationAsync(id, cancellationToken);
        return conversation is null ? NotFound() : Ok(conversation);
    }
}
