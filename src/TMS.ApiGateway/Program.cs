using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace TMS.ApiGateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Ocelot Basic setup
            builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddOcelot(); // single ocelot.json file in read-only mode

            builder.Services
                .AddOcelot(builder.Configuration);

            // Add your features
            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.AddConsole();
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            await app.UseOcelot();

            await app.RunAsync();
        }
    }
}
