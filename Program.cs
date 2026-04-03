using JobApplication.Data;
using JobApplication.Services.Applications;
using JobApplication.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace JobApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<AppDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IApplicationService, ApplicationService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddSwaggerGen(options =>
            {

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",       
                    In = ParameterLocation.Header, 
                    Type = SecuritySchemeType.Http, 
                    Scheme = "Bearer",             
                    BearerFormat = "JWT",          
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token."
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

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddCors(options => {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowAngular");
            app.MapControllers();

            app.Run();
        }
    }
}
