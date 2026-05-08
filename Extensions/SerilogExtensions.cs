using Serilog;

namespace JobApplication.Extensions
{
    public static class SerilogExtensions
    {
        public static WebApplicationBuilder AddSerilogConfig(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }
    }
}
