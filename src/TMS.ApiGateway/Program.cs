using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.Administration;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;
using TMS.ApiGateway.Extensions.Services;

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

        #region Ocelot + JWT

        // Проверка JWT-настроек
        var jwtKey = builder.Configuration["JWT_KEY"];
        var jwtIssuer = builder.Configuration["JWT_ISSUER"];
        var jwtAudience = builder.Configuration["JWT_AUDIENCE"];

        if (string.IsNullOrEmpty(jwtKey) ||
            string.IsNullOrEmpty(jwtIssuer) ||
            string.IsNullOrEmpty(jwtAudience))
        {
            throw new InvalidOperationException("JWT configuration is missing.");
        }

        // Проверка наличия ocelot.json
        if (!File.Exists(Path.Combine(builder.Environment.ContentRootPath, "ocelot.json")))
            throw new FileNotFoundException("ocelot.json не найден.");

        using var factory = LoggerFactory.Create(b => b.AddConsole());
        ILogger logger = factory.CreateLogger<Program>();

        builder.Services
            .AddOcelot(builder.Configuration)
            .AddAdministration("/administration", options => ConfigJwt(options, builder, jwtIssuer, jwtAudience, jwtKey, logger));

        #endregion Ocelot + JWT

        #region Ocelot + Swagger

        // Добавление сервисов
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
        });

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

    #region Private Methods

    private static void ConfigJwt(JwtBearerOptions options,
        WebApplicationBuilder builder,
        string jwtIssuer,
        string jwtAudience,
        string jwtKey,
        ILogger logger)
    {
        options.Authority = builder.Configuration["AuthService:Authority"];
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                logger.LogError("Authentication failed: {ex}", context.Exception);
                return Task.CompletedTask;
            }
        };
    }

    #endregion Private Methods
}