using System.Collections.Concurrent;

namespace TuitionApi.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, List<DateTime>> _requestTracker = new();
    private const int Limit = 3;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply to Mobile App Query Endpoint
        if (context.Request.Path.StartsWithSegments("/api/v1/mobile/tuition") && context.Request.Method == "GET")
        {
            // Extract StudentNo from path
            var path = context.Request.Path.Value;
            var studentNo = path?.Split('/').Last();

            if (!string.IsNullOrEmpty(studentNo))
            {
                var now = DateTime.UtcNow;
                var today = now.Date;

                _requestTracker.AddOrUpdate(studentNo, 
                    new List<DateTime> { now }, 
                    (key, list) => 
                    {
                        // Remove old entries (from previous days)
                        list.RemoveAll(d => d.Date < today);
                        list.Add(now);
                        return list;
                    });

                if (_requestTracker.TryGetValue(studentNo, out var requests))
                {
                    if (requests.Count(d => d.Date == today) > Limit)
                    {
                        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        await context.Response.WriteAsJsonAsync(new { error = "Rate limit exceeded. Max 3 requests per day." });
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}
