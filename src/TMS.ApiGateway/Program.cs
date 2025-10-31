using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using TMS.ApiGateway.gRpcClients;
using TMS.ApiGateway.Middlewares;

namespace TMS.ApiGateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor();
            
            builder.Services.AddMvc();
            
            builder.Services.AddEndpointsApiExplorer();

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

            builder.Services.AddTransient<IAuthClient, AuthClient>();

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

            // Сначала обрабатываем локальные маршруты
            app.MapWhen(context => context.Request.Path.StartsWithSegments("/"), appBuilder =>
            {
                appBuilder
                    .UseRouting()
                    .UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllerRoute(
                            name: "default",
                            pattern: "{controller=Home}/{action=Index}/{id?}");
                        endpoints.MapRazorPages();
                    });
            });

            // Затем настраиваем Ocelot для остальных маршрутов
            app.UseOcelot().Wait();

            var requireAuthorizationPaths = new[] { "/api/auth/users" };

            app.MapWhen(context => requireAuthorizationPaths.Any(ep => context.Request.Path.ToString().Contains(ep)), appBuilder =>
            {
                appBuilder.UseMiddleware<JwtMiddleware>();
            });

            await app.RunAsync();
        }
    }
}
