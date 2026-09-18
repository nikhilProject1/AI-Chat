using System.Text.Json;
using AiChat.Api.Services.Implementations;

namespace AiChat.Api.Middleware;

public sealed class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
                KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                LlmServiceException => (StatusCodes.Status502BadGateway, exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled request error.");
            }

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
        }
    }
}
