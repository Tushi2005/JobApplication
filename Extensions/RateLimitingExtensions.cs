using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace JobApplication.Extensions
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddRateLimitingConfig(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("login", config =>
                {
                    config.Window = TimeSpan.FromMinutes(1);
                    config.PermitLimit = 5;
                    config.QueueLimit = 0;
                    config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            return services;
        }
    }
}
