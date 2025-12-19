using Hangfire.Dashboard;

namespace Presentation.Middleware;

/// <summary>
/// Authorization filter for Hangfire Dashboard
/// Controls who can access the Hangfire dashboard at /hangfire
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// Determines whether the current user is authorized to view the Hangfire Dashboard
    /// </summary>
    /// <param name="context">The dashboard context</param>
    /// <returns>True if authorized, false otherwise</returns>
    public bool Authorize(DashboardContext context)
    {
        // TODO: For production, implement proper authentication
        // Example: Check if user is authenticated and has admin role
        // var httpContext = context.GetHttpContext();
        // return httpContext.User.Identity?.IsAuthenticated == true 
        //        && httpContext.User.IsInRole("Admin");

        // For development: Allow all access to Hangfire Dashboard
        return true;
    }
}
