using System.Net;
using System.Text.Json;
using Application.Common.Exceptions;

namespace Presentation.Middlewares;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and returns consistent error responses across the entire API
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continue to the next middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);

            // Handle the exception and return appropriate response
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            // 401 Unauthorized - User authentication failed
            UnauthorizedAccessException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = "Unauthorized access",
                Details = ex.Message
            },

            // 404 Not Found - Resource doesn't exist
            KeyNotFoundException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = "Resource not found",
                Details = ex.Message
            },

            // 400 Bad Request - Validation or business rule failure
            ValidationException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Validation failed",
                Details = ex.Message,
                Errors = ex.Errors // Include validation errors if available
            },

            // 400 Bad Request - Generic bad request
            ArgumentException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Invalid argument",
                Details = ex.Message
            },

            // 500 Internal Server Error - Unexpected errors
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An internal server error occurred",
                Details = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An error occurred while processing your request"
            }
        };

        // Set HTTP status code
        context.Response.StatusCode = response.StatusCode;

        // Include stack trace only in development
        if (_environment.IsDevelopment())
        {
            response.StackTrace = exception.StackTrace;
        }

        // Serialize and return error response
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Standard error response format for all API errors
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public string? StackTrace { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
