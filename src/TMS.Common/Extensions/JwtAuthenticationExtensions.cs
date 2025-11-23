using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TMS.Common.Extensions;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    ///
    /// </summary>
    public static void AddJwtAuthentication(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        var jwtKey = configuration["JWT_KEY"];
        var jwtIssuer = configuration["JWT_ISSUER"];
        var jwtAudience = configuration["JWT_AUDIENCE"];

        if (string.IsNullOrEmpty(jwtKey) ||
            string.IsNullOrEmpty(jwtIssuer) ||
            string.IsNullOrEmpty(jwtAudience))
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
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)
                    )
                };
            });
    }

    public static void ConfigJwt(JwtBearerOptions options,
        WebApplicationBuilder builder)
    {
        var jwtKey = builder.Configuration["JWT_KEY"];
        var jwtIssuer = builder.Configuration["JWT_ISSUER"];
        var jwtAudience = builder.Configuration["JWT_AUDIENCE"];
        var authority = builder.Configuration["AuthService:Authority"];

        if (string.IsNullOrEmpty(jwtKey) ||
            string.IsNullOrEmpty(jwtIssuer) ||
            string.IsNullOrEmpty(jwtAudience) ||
            string.IsNullOrEmpty(authority))
        {
            throw new Exception("JWT configuration is not properly set up");
        }

        options.Authority = authority;
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
    }
}
