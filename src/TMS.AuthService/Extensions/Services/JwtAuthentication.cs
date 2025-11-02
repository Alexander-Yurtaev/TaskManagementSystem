using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace TMS.AuthService.Extensions.Services;

/// <summary>
/// 
/// </summary>
public static class JwtAuthentication
{
    /// <summary>
    /// 
    /// </summary>
    public static void AddJwtAuthentication(this IServiceCollection services, 
        ConfigurationManager configuration,
        ILogger logger)
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

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)
                    )
                };
            });
    }
}
