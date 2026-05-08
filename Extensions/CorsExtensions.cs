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
                        "http://192.168.1.108:3000",
                        "http://localhost:4200"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
