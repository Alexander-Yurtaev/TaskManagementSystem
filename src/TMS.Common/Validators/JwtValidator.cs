using Microsoft.Extensions.Configuration;

namespace TMS.Common.Validators;

public static class JwtValidator
{
    public static ValidationResult JwtConfigurationValidate(IConfiguration configuration)
    {
        var jwtKey = configuration["JWT_KEY"];
        var jwtIssuer = configuration["JWT_ISSUER"];
        var jwtAudience = configuration["JWT_AUDIENCE"];
        var authority = configuration["JWT_AUTHORITY"];

        if (string.IsNullOrEmpty(jwtKey) ||
            string.IsNullOrEmpty(jwtIssuer) ||
            string.IsNullOrEmpty(jwtAudience) ||
            string.IsNullOrEmpty(authority))
        {
            return ValidationResult.Error("JWT configuration is not properly set up");
        }

        return ValidationResult.Success();
    }
}
