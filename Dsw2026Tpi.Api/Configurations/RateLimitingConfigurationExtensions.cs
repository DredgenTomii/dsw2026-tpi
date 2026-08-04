using Dsw2026Tpi.CrossCutting.Models;
using Dsw2026Tpi.CrossCutting.Resources;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Dsw2026Tpi.Api.Configurations;

public static class RateLimitingConfigurationExtensions
{
    public const string AdminLoginPolicy = "AdminLoginPolicy";
    public const string PatientLoginPolicy = "PatientLoginPolicy";
    public const string AppointmentsPolicy = "AppointmentsPolicy"; 

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter(AdminLoginPolicy, opt =>
            {
                opt.PermitLimit = configuration.GetValue<int>("RateLimiting:AdminLogin:PermitLimit");
                opt.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:AdminLogin:WindowSeconds"));
                opt.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter(PatientLoginPolicy, opt =>
            {
                opt.PermitLimit = configuration.GetValue<int>("RateLimiting:PatientLogin:PermitLimit");
                opt.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:PatientLogin:WindowSeconds"));
                opt.QueueLimit = 0;
            });

            options.AddPolicy(AppointmentsPolicy, context =>
            {
                var key = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anon";
                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = configuration.GetValue<int>("RateLimiting:Appointments:PermitLimit"),
                    Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:Appointments:WindowSeconds")),
                    QueueLimit = 0
                });
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var key = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anon";
                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = configuration.GetValue<int>("RateLimiting:Global:PermitLimit"),
                    Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:Global:WindowSeconds")),
                    QueueLimit = 0
                });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Rate limit excedido en {Path} desde {Ip}",
                    context.HttpContext.Request.Path, context.HttpContext.Connection.RemoteIpAddress);

                context.HttpContext.Response.ContentType = "application/json";
                var error = new ErrorResponse(nameof(ErrorCodes.RATE_LIMIT_EXCEEDED), ErrorCodes.RATE_LIMIT_EXCEEDED);
                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(error), cancellationToken);
            };
        });

        return services;
    }
}
