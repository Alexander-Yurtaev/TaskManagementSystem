using DotNetEnv;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Metrics;
using TMS.ApiGateway.Extensions.Services;
using TMS.Common.Helpers;
using System.Text;

namespace TMS.ApiGateway;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // автоматически ищет .env в текущей директории
        Env.Load();
        builder.Configuration.AddEnvironmentVariables();

        // HealthChecks
        builder.Services.AddHealthChecks()
            .AddUrlGroup(new Uri("http://tms-auth-service/health"), "auth-service")
            .AddUrlGroup(new Uri("http://tms-task-service/health"), "task-service")
            .AddUrlGroup(new Uri("http://tms-file-storage-service/health"), "file-service")
            .AddUrlGroup(new Uri("http://tms-notification-service/health"), "notification-service");

        // OpenTelemetry Metrics
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddMeter("Microsoft.AspNetCore.Hosting")
                       .AddMeter("Microsoft.AspNetCore.Server.Kestrel");
            });

        builder.Services.AddHttpContextAccessor();

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddConsole();
        }

        // Ocelot Basic setup
        builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddOcelot();

        // Проверка наличия ocelot.json
        if (!File.Exists(Path.Combine(builder.Environment.ContentRootPath, "ocelot.json")))
            throw new FileNotFoundException("ocelot.json не найден.");

        using var factory = LoggerFactory.Create(b => b.AddConsole());
        ILogger logger = factory.CreateLogger<Program>();

        #region Ocelot + JWT

        builder.Services
            .AddOcelot(builder.Configuration);

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => JwtHelper.ConfigJwt(options, builder.Configuration));

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        #endregion Ocelot + JWT

        #region Ocelot + Swagger

        // Добавление сервисов
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options => OpenApiSecurityHelper.AddSwaggerGenHelper(options, "TMS API", "v1"));

        // Настройка Ocelot + Swagger
        builder.Services.AddSwaggerForOcelot(builder.Configuration);

        #endregion Ocelot + Swagger

        // gRPC
        builder.Services.AddRpcConfiguration(builder.Configuration);

        // MVC
        builder.Services.AddMvc();

        // WebApplication
        var app = builder.Build();

        // Middleware: порядок критичен!
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseCors("AllowAll");

        // Swagger и UI
        app.UseSwagger();
        app.UseSwaggerForOcelotUI(opt =>
        {
            opt.PathToSwaggerGenerator = "/swagger/docs";
        });

        app.UseAuthentication();
        app.UseAuthorization();

        // РЕГИСТРАЦИЯ МАРШРУТОВ ВЕРХНЕГО УРОВНЯ

        // HealthChecks endpoint
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Metrics endpoint в формате Prometheus
        app.MapGet("/metrics", async context =>
        {
            context.Response.ContentType = "text/plain; version=0.0.4";

            var sb = new StringBuilder();

            // Базовые метрики системы
            sb.AppendLine("# HELP tms_http_requests_total Total HTTP requests to TMS Gateway");
            sb.AppendLine("# TYPE tms_http_requests_total counter");
            sb.AppendLine("tms_http_requests_total{service=\"gateway\",status=\"200\"} 0");

            sb.AppendLine("# HELP tms_memory_usage_bytes Memory usage in bytes");
            sb.AppendLine("# TYPE tms_memory_usage_bytes gauge");
            sb.AppendLine($"tms_memory_usage_bytes{{service=\"gateway\"}} {GC.GetTotalMemory(false)}");

            sb.AppendLine("# HELP tms_uptime_seconds Application uptime in seconds");
            sb.AppendLine("# TYPE tms_uptime_seconds gauge");
            sb.AppendLine($"tms_uptime_seconds{{service=\"gateway\"}} {Environment.TickCount / 1000}");

            sb.AppendLine("# HELP tms_gc_collections_total Garbage collector collections by generation");
            sb.AppendLine("# TYPE tms_gc_collections_total counter");
            sb.AppendLine($"tms_gc_collections_total{{service=\"gateway\",generation=\"0\"}} {GC.CollectionCount(0)}");
            sb.AppendLine($"tms_gc_collections_total{{service=\"gateway\",generation=\"1\"}} {GC.CollectionCount(1)}");
            sb.AppendLine($"tms_gc_collections_total{{service=\"gateway\",generation=\"2\"}} {GC.CollectionCount(2)}");

            await context.Response.WriteAsync(sb.ToString());
        });

        // Ocelot — ПОСЛЕ всех явных эндпоинтов
        await app.UseOcelot();

        await app.RunAsync();
    }
}