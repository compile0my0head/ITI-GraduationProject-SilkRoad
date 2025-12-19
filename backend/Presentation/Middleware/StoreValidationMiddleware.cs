using Application.Common.Interfaces;
using Infrastructure.Services;

namespace Presentation.Middleware;

/// <summary>
/// Store Validation Middleware - Validates that the store exists and user has access
/// This runs AFTER StoreContextMiddleware and AFTER Authentication
/// </summary>
public class StoreValidationMiddleware
{
    private readonly RequestDelegate _next;
    private const string StoreIdHeaderName = "X-Store-ID";

    // Non-store-scoped paths that should skip validation
    private static readonly string[] NonStoreScopedPaths = new[]
    {
        "/api/auth",
        "/api/users",
        "/api/stores/my",
        "/api/stores",
        "/api/teams/my",
        "/api/social-platforms", // Only the GET endpoint is global, but we check method in logic
        "/swagger",
        "/favicon.ico",
        "/_framework",
        "/_vs"
    };

    public StoreValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IStoreContext storeContext,
        IStoreAuthorizationService storeAuthorizationService,
        ICurrentUserService currentUserService)
    {
        // Check if this is a store-scoped endpoint
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var method = context.Request.Method.ToUpperInvariant();
        
        // Special case: GET /api/social-platforms is GLOBAL, but POST/PUT/DELETE are STORE-SCOPED
        if (path == "/api/social-platforms" && method == "GET")
        {
            await _next(context);
            return;
        }
        
        // Skip validation for non-store-scoped paths
        var isNonStoreScoped = NonStoreScopedPaths.Any(p => path.StartsWith(p.ToLowerInvariant()));

        // If non-store-scoped or no store context, continue without validation
        if (isNonStoreScoped || !storeContext.HasStoreContext)
        {
            await _next(context);
            return;
        }

        // Store-scoped endpoint with X-Store-ID header - validate access
        var storeId = storeContext.StoreId!.Value;
        var userId = currentUserService.UserId;

        // Check if user is authenticated
        if (userId == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "Authentication required for store-scoped operations"
            });
            return;
        }

        // Validate that user belongs to this store
        try
        {
            var hasAccess = await storeAuthorizationService.UserBelongsToStoreAsync(
                storeId, 
                userId.Value, 
                context.RequestAborted);

            if (!hasAccess)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Forbidden",
                    message = $"You do not have access to store {storeId}. You must be the store owner or a team member."
                });
                return;
            }
        }
        catch (KeyNotFoundException)
        {
            // Store doesn't exist
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Store Not Found",
                message = $"Store with ID {storeId} does not exist."
            });
            return;
        }
        catch (Exception ex)
        {
            // Log the error but don't expose internal details
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Internal Server Error",
                message = "An error occurred while validating store access."
            });
            return;
        }

        // Validation passed - continue to controller
        await _next(context);
    }
}

/// <summary>
/// Extension method to register the StoreValidation middleware
/// </summary>
public static class StoreValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseStoreValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<StoreValidationMiddleware>();
    }
}
