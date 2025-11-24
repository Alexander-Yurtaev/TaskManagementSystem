using DotNetEnv;
using System.Reflection;
using TMS.Common.Extensions;
using TMS.Common.Helpers;
using TMS.NotificationService.Data.Extensions;
using TMS.NotificationService.Extensions.ApiEndpoints;
using TMS.NotificationService.Extensions.Services;

namespace TMS.NotificationService
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

            // Add services to the container.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options => OpenApiSecurityHelper.AddSwaggerGenHelper(options, "Notify API", "v1", () =>
            {
                // Путь к XML-файлу (имя сборки)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            }));

            // Data Context configurations
            try
            {
                builder.Services.AddNotifyDataContext();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Ошибка при конфигурации TaskDataContext.");
                throw;
            }

            builder.Services.AddFileService("EmailEvents", service =>
            {
                service.BasePath = Environment.GetEnvironmentVariable("BASE_EVENTS_PATH")
                                   ??
                                   throw new InvalidOperationException("BASE_EVENTS_PATH does not defined.");
            });

            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddAuthorization();
            builder.Services.AddRabbitMqConsumerConfiguration();

            var app = builder.Build();

            logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Приложение успешно построено. Начинается настройка.");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notify API v1");
                    options.RoutePrefix = "swagger"; // URL: /swagger
                });
            }

            app.UseAuthentication();
            app.UseAuthorization();

            // Configure the HTTP request pipeline.
            app.AddGreetingEndpoint();
            app.AddMigrateEndpoint();

            app.UseHttpsRedirection();

            logger.LogInformation("Приложение успешно запущено.");

            app.Run();
        }
    }
}