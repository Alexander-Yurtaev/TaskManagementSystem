using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TMS.AuthService.Configurations;

namespace TMS.AuthService.Extensions.Services;

/// <summary>
/// 
/// </summary>
public static class JwtAuthentication
{
    /// <summary>
    /// 
    /// </summary>
    public static void AddJwtAuthentication(this IServiceCollection services, ConfigurationManager configuration)
    {
        var jwtKey = configuration["JWT_KEY"];
        var jwtConfig = configuration.GetSection("Jwt").Get<JwtOptions>()
                        ??
                        throw new Exception("JWT configuration is not properly set up");

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtConfig.Issuer) ||
            string.IsNullOrEmpty(jwtConfig.Audience))
        {
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
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)
                    )
                };
            });
    }
}
