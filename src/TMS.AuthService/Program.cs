using TMS.AuthService.Data.Extensions;
using TMS.AuthService.Endpoints;

namespace TMS.AuthService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddAuthDataContext();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.AddGreetingEndpoint();
            app.AddMigrateEndpoint();

            app.Run();
        }
    }
}
