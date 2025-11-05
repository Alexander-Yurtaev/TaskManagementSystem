using TMS.NotificationService.Data.Extensions;
using TMS.NotificationService.Extensions.Endpoints;
using TMS.NotificationService.Extensions.Services;

namespace TMS.NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddNotifyDataContext();

            builder.Services.AddRabbitMqConsumerConfiguration();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.AddGreetingEndpoint();

            app.Run();
        }
    }
}
