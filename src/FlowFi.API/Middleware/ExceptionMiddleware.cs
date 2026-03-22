using FluentValidation;
using System.Text.Json;

namespace FlowFi.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        => (_next, _logger) = (next, logger);

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (ValidationException ex)
        {
            context.Response.StatusCode  = 400;
            context.Response.ContentType = "application/json";
            var fields = ex.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage);
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                error   = new { code = "VALIDATION_ERROR", message = "Validation failed", fields }
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode  = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                error   = new { code = "INTERNAL_ERROR", message = "An unexpected error occurred" }
            }));
        }
    }
}
