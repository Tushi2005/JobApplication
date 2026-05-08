using JobApplication.Data;
using JobApplication.Exceptions;
using JobApplication.Extensions;
using JobApplication.Mappers;
using JobApplication.Services.Applications;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace JobApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddSerilogConfig();

            builder.Services.AddDatabase(builder.Configuration);
            builder.Services.AddIdentityConfig(builder.Configuration);
            builder.Services.AddSwaggerConfig();
            builder.Services.AddCorsConfig();
            builder.Services.AddRateLimitingConfig();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(
                        new Newtonsoft.Json.Converters.StringEnumConverter());
                });

            builder.Services.AddAutoMapper(typeof(AutoMapperApplication).Assembly);
            builder.Services.AddScoped<IApplicationService, ApplicationService>();

            // Cloudflare Tunnel a Docker hálózatból jön (172.x.x.x), nem loopbackről,
            // ezért a forwarded headers middleware-t explicit kell engedélyezni minden proxyra
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                           ForwardedHeaders.XForwardedProto |
                                           ForwardedHeaders.XForwardedHost;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var app = builder.Build();

            app.UseForwardedHeaders();

            app.UseExceptionHandler();
            app.UseSerilogRequestLogging();
            app.UseSwagger();
            app.UseSwaggerUI();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("AllowAngular");
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapCustomEndpoints();
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}
