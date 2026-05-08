using JobApplication.Data;
using JobApplication.Models;
using Microsoft.AspNetCore.Identity;

namespace JobApplication.Extensions
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddIdentityConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddApiEndpoints();

            services.AddAuthentication(IdentityConstants.BearerScheme)
                .AddBearerToken(IdentityConstants.BearerScheme, options =>
                {
                    options.BearerTokenExpiration = TimeSpan.FromHours(1);
                    options.RefreshTokenExpiration = TimeSpan.FromDays(14);
                })
                .AddCookie(IdentityConstants.ApplicationScheme)
                .AddCookie(IdentityConstants.ExternalScheme)
                .AddGoogle(options =>
                {
                    options.ClientId = configuration["Authentication:Google:ClientId"]!;
                    options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                });

            services.AddAuthorizationBuilder();

            return services;
        }
    }
}
