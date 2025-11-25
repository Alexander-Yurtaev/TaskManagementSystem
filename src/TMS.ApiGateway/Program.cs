using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using TMS.ApiGateway.Extensions.Services;
using TMS.Common.Helpers;

namespace TMS.ApiGateway;

public class Program
{
    public static async Task Main(string[] args)
    {
        // WebApplicationBuilder
        var builder = WebApplication.CreateBuilder(args);

        // автоматически ищет .env в текущей директории
        Env.Load();
        builder.Configuration.AddEnvironmentVariables();

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

        // Ocelot — после Swagger
        await app.UseOcelot();

        await app.RunAsync();
    }
}