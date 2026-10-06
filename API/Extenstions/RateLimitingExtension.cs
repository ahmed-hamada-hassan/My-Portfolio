using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace API.Extenstions;

public static class RateLimitingExtension
{
    public static IServiceCollection AddAppRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("Login", c =>
            {
                var ip = c.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 6,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                });
            });

            options.AddPolicy("PublicReading", c =>
            {
                var ip = c.Connection.RemoteIpAddress?.ToString() ?? "UnknownPublic";
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                });
            });

            options.AddPolicy("AdminManagement", c =>
            {
                var adminId = c.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownAdmin";
                return RateLimitPartition.GetFixedWindowLimiter(adminId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                });
            });

            options.AddPolicy("AdminReading", c =>
            {
                var adminId = c.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "UnknownAdmin";
                return RateLimitPartition.GetFixedWindowLimiter(adminId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                });
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds))
                        .ToString(CultureInfo.InvariantCulture);
                }

                context.HttpContext.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Title = "Too Many Requests",
                    Status = StatusCodes.Status429TooManyRequests,
                    Detail = "You have exceeded the allowed number of requests."
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, token);
            };
        });

        return services;
    }
}
