using Ocelot.Middleware;
using TMS.ApiGateway.Extensions.Services;
using TMS.ApiGateway.Middlewares;

namespace TMS.ApiGateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add your features
            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.AddConsole();
            }

            // Чтобы в контроллере узнать текущие схему и хост
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddMvc();

            builder.Services.AddEndpointsApiExplorer();

            // Ocelot Basic setup
            builder.Services.AddOcelotConfiguration(builder.Environment.ContentRootPath, builder.Configuration);

            // gRPC
            builder.Services.AddRpcConfiguration(builder.Configuration);

            //
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

            // Обрабатываем локальные маршруты
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

            // Настраиваем Ocelot для остальных маршрутов
            app.UseOcelot().Wait();

            // Устанавливаем middleware для путей, доступ к которым доступен только авторизованным пользователям
            var requireAuthorizationPaths = new[] { "/api/auth/users" };
            app.MapWhen(context => requireAuthorizationPaths.Any(ep => context.Request.Path.ToString().Contains(ep)), appBuilder =>
            {
                appBuilder.UseMiddleware<JwtMiddleware>();
            });

            await app.RunAsync();
        }
    }
}