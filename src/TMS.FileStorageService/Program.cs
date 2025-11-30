using DotNetEnv;
using TMS.FileStorageService.Extensions.ApiEndpoints;
using TMS.FileStorageService.Extensions.Services;

namespace TMS.FileStorageService
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // автоматически ищет .env в текущей директории
            Env.Load();
            builder.Configuration.AddEnvironmentVariables();

            using var factory = LoggerFactory.Create(b => b.AddConsole());
            ILogger logger = factory.CreateLogger<Program>();

            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.AddConsole();
            }

            builder.Services.AddHealthChecks();

            // Add services to the container.
            builder.Services.AddFileService();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.MapHealthChecks("/health");
            app.MapHealthChecks("/ready");
            app.MapHealthChecks("/live");

            app.AddGreetingEndpoint();
            app.AddFileStoragesEndpoint();

            app.Run();
        }
    }
}