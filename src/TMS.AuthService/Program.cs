using DotNetEnv;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TMS.AuthService.Data.Extensions;
using TMS.AuthService.Extensions.ApiEndpoints;
using TMS.AuthService.Extensions.ApiEndpoints.OperationFilters;
using TMS.AuthService.Extensions.Services;

namespace TMS.AuthService
{
    /// <summary>
    /// Точка входа в приложение Auth Service.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // автоматически ищет .env в текущей директории
            Env.Load();

            builder.Configuration.AddEnvironmentVariables();

            // Проверка обязательных настроек перед регистрацией сервисов
            ValidateConfiguration(builder.Configuration);

            // Add services to the container.
            builder.Services.AddGrpc();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "AuthService API",
                    Description = "Minimal API для Сервиса аутентификации."
                });

                // Добавляем фильтр операций здесь
                c.OperationFilter<AuthMigrationOperationFilter>();

                // Путь к XML-файлу (имя сборки)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddAuthorization();
            builder.Services.AddAuthDataContext();
            builder.Services.AddAuthServices();
            builder.Services.AddRedisConfiguration(builder.Configuration);

            var app = builder.Build();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Приложение успешно построено. Начинается настройка.");

            // Настройка middleware
            // Включение Swagger и SwaggerUI только в разработке
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
                    c.RoutePrefix = "swagger"; // URL: /swagger
                });
            }

            app.MapGrpcService<Services.Grpc.AuthService>();

            // Регистрация API-эндпоинтов
            app.AddGreetingEndpoint();
            app.AddMigrateEndpoint();
            app.AddAuthEndpoint();
            app.AddUserEndpoint();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            logger.LogInformation("Приложение успешно запущено.");

            app.Run();
        }

        /// <summary>
        /// Проверяет наличие обязательных настроек конфигурации.
        /// </summary>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <exception cref="ConfigurationException">Если обязательные настройки отсутствуют.</exception>
        private static void ValidateConfiguration(IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration["JWT_KEY"]))
                throw new ConfigurationException("Обязательный параметр JWT_KEY не задан в конфигурации.");
        }
    }
}
