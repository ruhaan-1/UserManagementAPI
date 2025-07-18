using System.Text;
using Microsoft.AspNetCore.Http;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // Log Request
        context.Request.EnableBuffering(); // Allows reading the body multiple times
        var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;

        _logger.LogInformation("Incoming Request: {method} {url} \nHeaders: {headers} \nBody: {body}",
            context.Request.Method,
            context.Request.Path,
            context.Request.Headers,
            requestBody);

        // Capture Response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context); // Call the next middleware

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation("Outgoing Response: {statusCode} \nBody: {body}",
            context.Response.StatusCode,
            responseText);

        await responseBody.CopyToAsync(originalBodyStream); // Write response back
    }
}