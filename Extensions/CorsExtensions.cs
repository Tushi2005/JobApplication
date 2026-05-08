namespace JobApplication.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsConfig(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins(
                        "http://localhost:4200",
                        "https://jobtracker.adamcloud.hu"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
