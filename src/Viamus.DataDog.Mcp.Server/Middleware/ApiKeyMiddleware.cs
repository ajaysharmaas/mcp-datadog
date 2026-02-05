using Microsoft.Extensions.Options;
using Viamus.DataDog.Mcp.Server.Configuration;

namespace Viamus.DataDog.Mcp.Server.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOptionsMonitor<ServerSecuritySettings> settings)
    {
        var securitySettings = settings.CurrentValue;

        // Skip validation if API key is not required
        if (!securitySettings.RequireApiKey)
        {
            await _next(context);
            return;
        }

        // Allow health check endpoint without API key
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        // Check for API key in header
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedApiKey))
        {
            _logger.LogWarning("API key missing from request to {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "API key is required" });
            return;
        }

        // Validate API key
        if (string.IsNullOrWhiteSpace(securitySettings.ApiKey) ||
            !string.Equals(providedApiKey, securitySettings.ApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid API key provided for request to {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        await _next(context);
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyMiddleware>();
    }
}
