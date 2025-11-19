using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using TMS.ApiGateway.Extensions.Services;
using TMS.Common.Extensions;
using TMS.Common.Helpers;

namespace TMS.ApiGateway;

public class Program
{
    public static async Task Main(string[] args)
    {
        // WebApplicationBuilder
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpContextAccessor();

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddConsole();
        }

        // Ocelot Basic setup
        builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddOcelot(); // single ocelot.json file in read-only mode

        // Проверка наличия ocelot.json
        if (!File.Exists(Path.Combine(builder.Environment.ContentRootPath, "ocelot.json")))
            throw new FileNotFoundException("ocelot.json не найден.");

        using var factory = LoggerFactory.Create(b => b.AddConsole());
        ILogger logger = factory.CreateLogger<Program>();

        #region Ocelot + JWT

        // Проверка JWT-настроек
        var jwtKey = builder.Configuration["JWT_KEY"];
        var jwtIssuer = builder.Configuration["JWT_ISSUER"];
        var jwtAudience = builder.Configuration["JWT_AUDIENCE"];
        var authority = builder.Configuration["AuthService:Authority"];

        if (string.IsNullOrEmpty(jwtKey) ||
            string.IsNullOrEmpty(jwtIssuer) ||
            string.IsNullOrEmpty(jwtAudience) ||
            string.IsNullOrEmpty(authority))
        {
            throw new InvalidOperationException("JWT configuration is missing.");
        }

        builder.Services
            .AddOcelot(builder.Configuration);

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => JwtAuthenticationExtensions.ConfigJwt(options, builder, jwtIssuer, jwtAudience, jwtKey, logger));

        #endregion Ocelot + JWT

        #region Ocelot + Swagger

        // Добавление сервисов
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options => OpenApiHelper.AddSwaggerGenHelper(options));

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
        app.UseAuthentication();
        app.UseAuthorization();

        #region Swagger + Ocelot

        // Swagger и UI
        app.UseSwagger();
        app.UseSwaggerForOcelotUI(opt =>
        {
            opt.PathToSwaggerGenerator = "/swagger/docs";
        });

        // Ocelot — после Swagger
        await app.UseOcelot();

        #endregion Swagger + Ocelot

        await app.RunAsync();
    }
}