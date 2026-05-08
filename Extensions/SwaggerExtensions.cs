using Microsoft.OpenApi;

namespace JobApplication.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    Description = "Bearer token authorization."
                });

                options.AddSecurityRequirement(doc =>
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecuritySchemeReference("Bearer", doc),
                            new List<string>()
                        }
                    }
                );
            });

            services.AddEndpointsApiExplorer();

            return services;
        }
    }
}
