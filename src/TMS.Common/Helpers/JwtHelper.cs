using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TMS.Common.Validators;

namespace TMS.Common.Helpers;

public static class JwtHelper
{
    public static void ConfigJwt(JwtBearerOptions options,
        IConfiguration configuration)
    {
        var validationResult = JwtValidator.JwtConfigurationValidate(configuration);
        if (!validationResult.IsValid)
        {
            throw new Exception(validationResult.ErrorMessage);
        }

        var jwtKey = configuration["JWT_KEY"]!;
        var jwtIssuer = configuration["JWT_ISSUER"];
        var jwtAudience = configuration["JWT_AUDIENCE"];
        var authority = configuration["AuthService:Authority"];

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
