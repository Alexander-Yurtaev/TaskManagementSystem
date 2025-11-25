using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TMS.Common.Validators;

namespace TMS.Common.Extensions;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    ///
    /// </summary>
    public static void AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var validationResult = JwtValidator.JwtConfigurationValidate(configuration);
        if (!validationResult.IsValid)
        {
            throw new Exception(validationResult.ErrorMessage);
        }

        var jwtKey = configuration["JWT_KEY"]!;
        var jwtIssuer = configuration["JWT_ISSUER"];
        var jwtAudience = configuration["JWT_AUDIENCE"];
        var authority = configuration["JWT_AUTHORITY"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
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
