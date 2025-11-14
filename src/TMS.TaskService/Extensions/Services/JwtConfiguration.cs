using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TMS.TaskService.Extensions.Services;

/// <summary>
///
/// </summary>
public static class JwtConfiguration
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="contentRootPath"></param>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    /// <exception cref="Exception"></exception>
    public static void AddJwtConfiguration(this IServiceCollection services,
        string contentRootPath, ConfigurationManager configuration, ILogger logger)
    {
        var jwtKey = configuration["JWT_KEY"];
        var jwtIssuer = configuration["JWT_ISSUER"];
        var jwtAudience = configuration["JWT_AUDIENCE"];

        if (string.IsNullOrEmpty(jwtKey) ||
            string.IsNullOrEmpty(jwtIssuer) ||
            string.IsNullOrEmpty(jwtAudience))
        {
            logger.LogError("JWT configuration is not properly set up");
            throw new Exception("JWT configuration is not properly set up");
        }

        configuration
            .SetBasePath(contentRootPath);

        void Options(JwtBearerOptions o)
        {
            o.Authority = configuration["TaskService:Authority"];
            o.RequireHttpsMetadata = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
            o.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    logger.LogError("Authentication failed: {Exception}", context.Exception);
                    return Task.CompletedTask;
                }
            };
        }

        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", Options);
    }
}