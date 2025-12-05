using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using System.Reflection;
using System.Security.Claims;
using TMS.AuthService.Data;
using TMS.AuthService.Data.Extensions;
using TMS.AuthService.Extensions.ApiEndpoints;
using TMS.AuthService.Extensions.Services;
using TMS.Common.Extensions;
using TMS.Common.Helpers;
using TMS.Common.Validators;

namespace TMS.AuthService;

/// <summary>
/// Точка входа в приложение Auth Service.
/// </summary>
public class Program
{
    const string TargetMigration = "SeedSuperadminUser";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // автоматически ищет .env в текущей директории
        Env.Load();
        builder.Configuration.AddEnvironmentVariables();

        // Проверяем аргументы командной строки
        if (args.Contains("--setup") || args.Contains("setup"))
        {
            await MigrationExtensions.RunMigrations<AuthDataContext>(builder.Configuration,
                PostgresHelper.GetConnectionString(),
                TargetMigration);
            return;
        }

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

        builder.Services.AddAuthorization(options =>
        {
            // Политика для регистрации пользователей
            options.AddPolicy("CanRegisterUsers", policy => policy.RequireRole("Admin"));
            
            // Политика для регистрации администраторов
            options.AddPolicy("CanRegisterAdmins", policy => policy.RequireRole("SuperAdmin"));

            //
            options.AddPolicy("AllowRegistion", policy =>
            {
                policy.RequireAssertion(context => 
                    context.User.HasClaim(c => c.Type == ClaimTypes.Role && 
                                    (c.Value == "Admin" || c.Value == "SuperAdmin")));
            });

            //
            options.AddPolicy("AllowDeletion", policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == ClaimTypes.Role &&
                                    (c.Value == "Admin" || c.Value == "SuperAdmin")));
            });

            options.AddPolicy("CanMigrate", policy => policy.RequireRole("Admin"));
        });
        builder.Services.AddAuthDataContext();
        builder.Services.AddAuthServices();

        var connectionString = PostgresHelper.GetConnectionString();
        var connectionMultiplexer = builder.Services.AddRedisConfiguration(builder.Configuration);
        
        builder.Services
            .AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: ["db", "sql", "postgres"])
            .AddRedis(connectionMultiplexer, "redis")
            .ForwardToPrometheus();

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

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/ready");
        app.MapHealthChecks("/live");

        // Регистрация API-эндпоинтов
        app.AddGreetingEndpoint();
        app.AddMigrateEndpoint();
        app.AddAuthEndpoint();
        app.AddUserEndpoint();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        await app.RunAsync();
    }
}
