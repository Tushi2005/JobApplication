using JobApplication.Data;
using JobApplication.Exceptions;
using JobApplication.Extensions;
using JobApplication.Mappers;
using JobApplication.Services.Applications;
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

            var app = builder.Build();

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
