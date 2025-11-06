using TMS.Common.Extensions;
using TMS.FileStorageService.Extensions.Endpoints;

namespace TMS.FileStorageService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddFileService("AttachmentFiles", service =>
            {
                service.BasePath = Environment.GetEnvironmentVariable("BASE_FILES_PATH")
                                      ??
                                      throw new InvalidOperationException("BASE_EVENTS_PATH does not defined.");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.AddGreetingEndpoint();
            app.AddFileStoragesEndpoint();

            app.Run();
        }
    }
}
