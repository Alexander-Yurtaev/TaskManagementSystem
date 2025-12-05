using System.IdentityModel.Tokens.Jwt;
using TMS.Common.Enums;
using TMS.Common.Models;

namespace TMS.WebApp.Services;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://tms-gateway:8081/api/auth";

            var request = new LoginModel(username, password);

            _logger.LogInformation($"Attempting login for user: {username}");
            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/login", request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<TokensModel>();

                _logger.LogInformation($"Login successful for user: {username}");
                _logger.LogDebug($"Access token length: {authResponse?.AccessToken?.Length ?? 0}");
                _logger.LogDebug($"Refresh token length: {authResponse?.RefreshToken?.Length ?? 0}");

                if (authResponse is null)
                    return false;

                // Сохраняем токены в сессию
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext is not null)
                {
                    // Проверяем доступность сессии
                    if (!httpContext.Session.IsAvailable)
                    {
                        _logger.LogWarning("Session is not available!");
                    }
                    else
                    {
                        httpContext.Session.SetString("access_token", authResponse?.AccessToken ?? "");
                        httpContext.Session.SetString("refresh_token", authResponse?.RefreshToken ?? "");
                        _logger.LogInformation($"Tokens saved to session. Session ID: {httpContext.Session.Id}");

                        // Также сохраняем в куки на всякий случай
                        httpContext.Response.Cookies.Append("access_token", authResponse?.AccessToken ?? "", new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = httpContext.Request.IsHttps,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddDays(1)
                        });
                    }
                }

                return true;
            }

            _logger.LogWarning($"Login failed for user: {username}. Status: {response.StatusCode}");

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return false;
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return false;
            }

            var refreshToken = httpContext.Session.GetString("refresh_token");
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://tms-gateway:8081/api/auth";

            var request = new { RefreshToken = refreshToken };
            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/refresh", request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<TokensModel>();
                if (authResponse is not null)
                {
                    httpContext.Session.SetString("access_token", authResponse.AccessToken);
                    httpContext.Session.SetString("refresh_token", authResponse.RefreshToken);
                    return true;
                }

                return false;
            }

            // НЕ очищаем сессию при ошибке refresh
            // Logout(); // ← ЗАКОММЕНТИРОВАТЬ эту строку

            _logger.LogWarning($"Token refresh failed with status: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            // Logout(); // ← И здесь тоже
            return false;
        }
    }

    public bool IsAuthenticated()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return false;
        }

        var token = httpContext.Session.GetString("access_token");
        if (string.IsNullOrEmpty(token))
            return false;

        // Проверяем, не истек ли токен
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
            return false;

        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.ValidTo > DateTime.UtcNow.AddMinutes(1);
    }

    public string GetAccessToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Session.GetString("access_token") ?? string.Empty;
    }

    public void Logout()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            httpContext.Session.Remove("access_token");
            httpContext.Session.Remove("refresh_token");
        }
    }

    public UserRole? GetCurrentUserRole()
    {
        try
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
            {
                return null; 
            }

            var jwtToken = handler.ReadJwtToken(token);

            // Получаем роль из claims (пример, зависит от того, как настроен ваш backend)
            var json = System.Text.Json.JsonSerializer.Serialize(jwtToken.Claims);
            
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.ToLower() == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            if (roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, out var role))
            {
                return role;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user role from token");
            return null;
        }
    }

    public async Task<bool> RegistrationAsync(string username, string email, string password, UserRole role)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AuthenticatedClient");
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://tms-gateway:8081/api/register";

            var request = new RegisterModel(username, email, password, role);

            _logger.LogInformation($"Attempting register for user: {username}");
            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/register", request);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            _logger.LogWarning($"Login failed for user: {username}. Status: {response.StatusCode}");

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return false;
        }
    }
}