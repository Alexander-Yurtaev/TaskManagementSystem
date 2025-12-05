// Services/AuthService.cs
namespace TMS.WebApp.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task<bool> RefreshTokenAsync();
    bool IsAuthenticated();
    string GetAccessToken();
    void Logout();
}
