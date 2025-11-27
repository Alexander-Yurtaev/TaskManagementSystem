using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection;
using TMS.AuthService.Data.Extensions;
using TMS.AuthService.Extensions.ApiEndpoints;
using TMS.AuthService.Extensions.Services;
using TMS.Common.Helpers;
using TMS.Common.Validators;

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
            JwtValidator.ThrowIfNotValidate(builder.Configuration);

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => JwtHelper.ConfigJwt(options, builder.Configuration));

            // Add services to the container.
            builder.Services.AddGrpc();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options => OpenApiSecurityHelper.AddSwaggerGenHelper(options, "Auth API", "v1", () =>
            {
                // Путь к XML-файлу (имя сборки)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            }));

            builder.Services.AddAuthorization();
            builder.Services.AddAuthDataContext();
            builder.Services.AddAuthServices();
            builder.Services.AddRedisConfiguration(builder.Configuration);

            var app = builder.Build();

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

            app.Run();
        }
    }
}
