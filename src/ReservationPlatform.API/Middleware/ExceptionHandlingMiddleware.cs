using System.Text.Json;
using ReservationPlatform.Application.Common.Exceptions;
using ValidationException = ReservationPlatform.Application.Common.Exceptions.ValidationException;

namespace ReservationPlatform.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (StatusCodes.Status422UnprocessableEntity, "Validation failed.", ve.Errors),
            NotFoundException nfe => (StatusCodes.Status404NotFound, nfe.Message, (IDictionary<string, string[]>?)null),
            ConflictException ce => (StatusCodes.Status409Conflict, ce.Message, null),
            ForbiddenException fe => (StatusCodes.Status403Forbidden, fe.Message, null),
            InvalidOperationException ioe => (StatusCodes.Status400BadRequest, ioe.Message, null),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized.", null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null)
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            success = false,
            message,
            errors,
            data = (object?)null
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
