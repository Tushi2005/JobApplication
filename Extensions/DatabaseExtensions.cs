using JobApplication.Data;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = configuration["DatabaseProvider"];
            var connectionString = configuration.GetConnectionString(provider!);

            services.AddDbContext<AppDbContext>(options =>
            {
                switch (provider)
                {
                    case "SqlServer":
                        options.UseSqlServer(connectionString);
                        break;
                    case "Postgres":
                        options.UseNpgsql(connectionString);
                        break;
                    case "Azure":
                        options.UseAzureSql(connectionString);
                        break;
                    default:
                        throw new InvalidOperationException($"Ismeretlen provider: {provider}");
                }
            });

            return services;
        }
    }
}
