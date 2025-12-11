using System.Net;
using System.Text.Json;
using DomainDrivenLibrary.Contracts.Responses;

namespace DomainDrivenLibrary.Middleware;

/// <summary>
///     Middleware for handling exceptions and converting them to appropriate HTTP responses.
///     Provides consistent error handling across all API endpoints.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, error, message) = MapException(exception);

        logger.LogError(
            exception,
            "Request {Method} {Path} failed with {StatusCode}: {Message}",
            context.Request.Method,
            context.Request.Path,
            statusCode,
            message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse(error, message);
        var json = JsonSerializer.Serialize(response, JsonOptions);

        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode StatusCode, string Error, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            InvalidOperationException ex when IsNotFoundError(ex.Message) =>
                (HttpStatusCode.NotFound, "NotFound", ex.Message),

            InvalidOperationException ex when IsConflictError(ex.Message) =>
                (HttpStatusCode.Conflict, "Conflict", ex.Message),

            InvalidOperationException ex =>
                (HttpStatusCode.BadRequest, "InvalidOperation", ex.Message),

            ArgumentException ex =>
                (HttpStatusCode.BadRequest, "ValidationError", ex.Message),

            _ => (HttpStatusCode.InternalServerError, "InternalError", "An unexpected error occurred.")
        };
    }

    private static bool IsNotFoundError(string message)
    {
        return message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("was not found", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsConflictError(string message)
    {
        return message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("already registered", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("already borrowed", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("not currently borrowed", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
///     Extension methods for registering the exception handling middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
