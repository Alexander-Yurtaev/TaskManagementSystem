using DotNetEnv;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TMS.Common.Extensions;
using TMS.NotificationService.Data.Extensions;
using TMS.NotificationService.Extensions.ApiEndpoints;
using TMS.NotificationService.Extensions.ApiEndpoints.OperationFilters;
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
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "AuthService API",
                    Description = "Minimal API для Сервиса рассылки сообщений."
                });

                // Добавляем фильтр операций здесь
                c.OperationFilter<NotifyMigrationOperationFilter>();

                // Путь к XML-файлу (имя сборки)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

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

            builder.Services.AddRabbitMqConsumerConfiguration();

            var app = builder.Build();

            logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Приложение успешно построено. Начинается настройка.");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
                    options.RoutePrefix = "swagger"; // URL: /swagger
                });
            }

            // Configure the HTTP request pipeline.
            app.AddGreetingEndpoint();
            app.AddMigrateEndpoint();

            app.UseHttpsRedirection();

            logger.LogInformation("Приложение успешно запущено.");

            app.Run();
        }
    }
}