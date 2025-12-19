using Infrastructure.Services;

namespace Presentation.Middleware;

/// <summary>
/// Store Context Middleware - Extracts Store ID from HTTP header and sets it in StoreContext
/// Header name: "X-Store-ID"
/// This middleware runs early in the pipeline to make StoreId available to all subsequent middleware and services
/// </summary>
public class StoreContextMiddleware
{
    private readonly RequestDelegate _next;
    private const string StoreIdHeaderName = "X-Store-ID";

    public StoreContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, StoreContext storeContext)
    {
        // Try to read Store ID from HTTP header
        if (context.Request.Headers.TryGetValue(StoreIdHeaderName, out var storeIdHeader))
        {
            var storeIdValue = storeIdHeader.FirstOrDefault();
            
            // Validate and parse the GUID
            if (!string.IsNullOrWhiteSpace(storeIdValue) && Guid.TryParse(storeIdValue, out var storeId))
            {
                // Set the Store ID in the scoped StoreContext
                storeContext.SetStoreId(storeId);
            }
            else if (!string.IsNullOrWhiteSpace(storeIdValue))
            {
                // Invalid GUID format - return 400 Bad Request
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid Store ID format",
                    message = $"The '{StoreIdHeaderName}' header must contain a valid GUID"
                });
                return;
            }
        }

        // Continue to next middleware
        // Note: If header is missing, StoreId remains null (which is valid for non-store-scoped endpoints)
        await _next(context);
    }
}

/// <summary>
/// Extension method to register the StoreContext middleware
/// </summary>
public static class StoreContextMiddlewareExtensions
{
    public static IApplicationBuilder UseStoreContext(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<StoreContextMiddleware>();
    }
}
