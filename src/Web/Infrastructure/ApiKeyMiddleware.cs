using Microsoft.Extensions.Options;
using WeatherLens.Application;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiOptions> apiSettings)
    {
        _next = next;
        _apiKey = apiSettings.Value.ApiKey;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the request path is for Swagger
        var path = context.Request.Path.Value;
        if (path == "/" || path!.StartsWith("/swagger") || path.StartsWith("/swaggerui") || path.StartsWith("/api/docs"))
        {
            await _next(context); // Skip API key validation for Swagger and root
            return;
        }
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey) ||
            !string.Equals(extractedApiKey, _apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized request.");
            return;
        }

        await _next(context);
    }
}
