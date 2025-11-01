using TMS.AuthService.Data;
using TMS.AuthService.Services;

namespace TMS.AuthService.Extensions;

/// <summary>
/// 
/// </summary>
public static class AuthServices
{
    /// <summary>
    /// 
    /// </summary>
    public static void AddAuthServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IHashService, HashService>();
    }
}