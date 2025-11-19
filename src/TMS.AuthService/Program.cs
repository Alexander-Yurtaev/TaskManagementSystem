using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TMS.AuthService.Data.Extensions;
using TMS.AuthService.Extensions.ApiEndpoints;
using TMS.AuthService.Extensions.Services;
using TMS.Common.Extensions;
using TMS.Common.Helpers;

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

            using var factory = LoggerFactory.Create(b => b.AddConsole());
            ILogger logger = factory.CreateLogger<Program>();

            // автоматически ищет .env в текущей директории
            Env.Load();

            builder.Configuration.AddEnvironmentVariables();

            // Проверка обязательных настроек перед регистрацией сервисов
            ValidateConfiguration(builder.Configuration);

            // Проверка JWT-настроек
            var jwtKey = builder.Configuration["JWT_KEY"]!;
            var jwtIssuer = builder.Configuration["JWT_ISSUER"]!;
            var jwtAudience = builder.Configuration["JWT_AUDIENCE"]!;

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => JwtAuthenticationExtensions.ConfigJwt(options, builder, jwtIssuer, jwtAudience, jwtKey, logger));

            // Add services to the container.
            builder.Services.AddGrpc();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options => OpenApiHelper.AddSwaggerGenHelper(options, () =>
            {
                // Путь к XML-файлу (имя сборки)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            }));

            //builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddAuthorization();
            builder.Services.AddAuthDataContext();
            builder.Services.AddAuthServices();
            builder.Services.AddRedisConfiguration(builder.Configuration);

            var app = builder.Build();

            logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Приложение успешно построено. Начинается настройка.");

            // Настройка middleware
            // Включение Swagger и SwaggerUI только в разработке
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
                    options.RoutePrefix = "swagger"; // URL: /swagger
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
            var jwtKey = configuration["JWT_KEY"];
            var jwtIssuer = configuration["JWT_ISSUER"];
            var jwtAudience = configuration["JWT_AUDIENCE"];
            var authority = configuration["AuthService:Authority"];

            if (string.IsNullOrEmpty(jwtKey) ||
                string.IsNullOrEmpty(jwtIssuer) ||
                string.IsNullOrEmpty(jwtAudience) ||
                string.IsNullOrEmpty(authority))
            {
                throw new InvalidOperationException("JWT configuration is missing.");
            }
        }
    }
}
