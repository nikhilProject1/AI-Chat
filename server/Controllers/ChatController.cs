using AiChat.Api.DTOs;
using AiChat.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ChatController(IChatService chatService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ChatResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ChatResponse>> SendMessage(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        var response = await chatService.SendMessageAsync(request, cancellationToken);
        return Ok(response);
    }
}
