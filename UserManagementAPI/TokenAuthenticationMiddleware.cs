using System.Text.Json;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenAuthenticationMiddleware> _logger;

    // Your fake token for demo purposes
    private const string ValidToken = "Bearer secret-token-123";

    public TokenAuthenticationMiddleware(RequestDelegate next, ILogger<TokenAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authHeader != ValidToken)
        {
            _logger.LogWarning("Unauthorized request. Header: {authHeader}", authHeader);

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var errorResponse = new { error = "Unauthorized" };
            var json = JsonSerializer.Serialize(errorResponse);

            await context.Response.WriteAsync(json);
            return;
        }

        await _next(context);
    }
}