using DotNetEnv;
using RabbitMQ.Client;
using Prometheus;
using TMS.Common.Extensions;
using TMS.Common.Helpers;
using TMS.NotificationService.Data.Extensions;
using TMS.NotificationService.Extensions.ApiEndpoints;
using TMS.NotificationService.Extensions.Services;
using TMS.NotificationService.Data;

namespace TMS.NotificationService;

/// <summary>
/// 
/// </summary>
public class Program
{
    const string TargetMigration = "InitialMigrations";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // автоматически ищет .env в текущей директории
        Env.Load();
        builder.Configuration.AddEnvironmentVariables();

        // Проверяем аргументы командной строки
        if (args.Contains("--setup") || args.Contains("setup"))
        {
            await MigrationExtensions.RunMigrations<NotificationDataContext>(builder.Configuration,
                PostgresHelper.GetConnectionString(),
                TargetMigration);
            return;
        }

        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        ILogger logger = loggerFactory.CreateLogger<Program>();

        // Add services to the container.
        builder.Services.AddEndpointsApiExplorer();

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

        builder.Services.AddEmailService();

        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddAuthorization();
        builder.Services.AddRabbitMqConsumerConfiguration();

        var rabbitMqFactoryResult = RabbitMQHelper.CreateConnectionFactory(logger);

        if (!rabbitMqFactoryResult.Item1.IsValid)
        {
            throw new Exception(rabbitMqFactoryResult.Item1.ErrorMessage);
        }

        var connectionString = PostgresHelper.GetConnectionString();

        builder.Services
            .AddSingleton(sp =>
            {
                var factory = new ConnectionFactory
                {
                    Uri = rabbitMqFactoryResult.Item2!.Uri,
                };
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            })
            .AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: ["db", "sql", "postgres"])
            .AddRabbitMQ(name: "rabbit")
            .ForwardToPrometheus();

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/ready");
        app.MapHealthChecks("/live");

        // Configure the HTTP request pipeline.
        app.AddGreetingEndpoint();
        app.AddMigrateEndpoint();

        app.UseHttpsRedirection();

        await app.RunAsync();
    }
}