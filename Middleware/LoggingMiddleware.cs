using System.Diagnostics;

namespace TuitionApi.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;
        
        // Log Request
        var headers = string.Join("; ", request.Headers.Select(h => $"{h.Key}={h.Value}"));
        _logger.LogInformation($"Incoming Request: {request.Method} {request.Path} from {context.Connection.RemoteIpAddress}. Size: {request.ContentLength ?? 0} bytes. Headers: {headers}");

        await _next(context);

        stopwatch.Stop();
        var response = context.Response;
        var authStatus = context.User.Identity?.IsAuthenticated == true ? "Authenticated" : "Not Authenticated";

        // Log Response
        _logger.LogInformation($"Response: {response.StatusCode} in {stopwatch.ElapsedMilliseconds}ms. Size: {response.ContentLength ?? 0} bytes. Auth: {authStatus}");
    }
}
