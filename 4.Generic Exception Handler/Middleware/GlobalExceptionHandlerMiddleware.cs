using System.Net;
using System.Text.Json;
using GenericExceptionHandler.Exceptions;
using GenericExceptionHandler.Models;

namespace GenericExceptionHandler.Middleware;

/// <summary>
/// Middleware for handling exceptions globally across the application
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Initializes a new instance of the GlobalExceptionHandlerMiddleware
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="env">The web host environment</param>
    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    /// <param name="context">The HTTP context</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log request details
        _logger.LogError(
            "Request {Method} {Path}{QueryString} failed with exception: {Message}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            exception.Message);

        // Log stack trace only in development
        if (_env.IsDevelopment())
        {
            _logger.LogError(exception.StackTrace);
        }

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            ValidationException validationEx => new ErrorResponse(
                (int)HttpStatusCode.BadRequest,
                "Validation failed",
                validationEx.Errors),

            BusinessLogicException businessEx => new ErrorResponse(
                (int)HttpStatusCode.Conflict,
                businessEx.Message),

            NotFoundException notFoundEx => new ErrorResponse(
                (int)HttpStatusCode.NotFound,
                notFoundEx.Message),

            _ => new ErrorResponse(
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred")
        };

        response.StatusCode = errorResponse.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}
