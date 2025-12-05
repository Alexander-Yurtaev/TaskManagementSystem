// Services/AuthService.cs
using TMS.Common.Enums;

namespace TMS.WebApp.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task<bool> RefreshTokenAsync();
    bool IsAuthenticated();
    string GetAccessToken();
    void Logout();

    Task<bool> RegistrationAsync(string username, string email, string password, UserRole role);

    UserRole? GetCurrentUserRole();
}
