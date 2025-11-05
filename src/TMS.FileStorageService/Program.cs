using TMS.FileStorageService.Extensions;

namespace TMS.FileStorageService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            using var factory = LoggerFactory.Create(b => b.AddConsole());
            ILogger logger = factory.CreateLogger<Program>();

            app.CreateFilePath(logger);

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGet("/", () => "Hello from FileStorageService!");

            app.Run();
        }
    }
}
