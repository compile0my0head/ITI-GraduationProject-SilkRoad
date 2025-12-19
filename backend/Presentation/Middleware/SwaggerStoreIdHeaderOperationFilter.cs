using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.Middleware;

/// <summary>
/// Swagger Operation Filter - Adds X-Store-ID header parameter to store-scoped endpoints only
/// This allows developers to easily test store-scoped endpoints
/// </summary>
public class SwaggerStoreIdHeaderOperationFilter : IOperationFilter
{
    // Non-store-scoped endpoints that should NOT have X-Store-ID header
    private static readonly string[] NonStoreScopedPaths = new[]
    {
        "api/auth",           // Authentication endpoints (login, register, logout)
        "api/users",          // User management (global, not store-specific)
        "api/stores/my",      // Get user's accessible stores (needs to return all stores)
        "api/stores",         // Store CRUD operations (getting/creating stores is non-scoped)
        "api/teams/my"        // Get user's teams across all stores (global)
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Get the request path (Swagger provides it without leading slash)
        var path = context.ApiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? string.Empty;
        
        // Special case: GET /api/social-platforms is GLOBAL (platform discovery)
        if (path == "api/social-platforms" && method == "GET")
        {
            return; // Don't add X-Store-ID header
        }
        
        // Check if this is a non-store-scoped endpoint
        foreach (var nonScopedPath in NonStoreScopedPaths)
        {
            // Match if path starts with the non-scoped pattern
            if (path.StartsWith(nonScopedPath, StringComparison.OrdinalIgnoreCase))
            {
                // This is a non-store-scoped endpoint - do NOT add X-Store-ID header
                return;
            }
        }

        // This is a store-scoped endpoint - add X-Store-ID header
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Store-ID",
            In = ParameterLocation.Header,
            Description = "Store ID (GUID) for store-scoped operations. Required after selecting a store.",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid"
            }
        });
    }
}
