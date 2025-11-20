using Microsoft.Extensions.Configuration;

namespace TMS.Common.Helpers;

public static class JwtHelper
{
    public static void ValidateConfiguration(IConfiguration configuration)
    {
        var jwtKey = configuration["JWT_KEY"];
        var jwtIssuer = configuration["JWT_ISSUER"];
        var jwtAudience = configuration["JWT_AUDIENCE"];
        var authority = configuration["AuthService:Authority"];

        if (string.IsNullOrEmpty(jwtKey) ||
            string.IsNullOrEmpty(jwtIssuer) ||
            string.IsNullOrEmpty(jwtAudience) ||
            string.IsNullOrEmpty(authority))
        {
            throw new InvalidOperationException("JWT configuration is missing.");
        }
    }
}
