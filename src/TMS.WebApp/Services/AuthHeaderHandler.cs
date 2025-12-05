using System.Net.Http.Headers;

namespace TMS.WebApp.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthHeaderHandler> _logger;

    public AuthHeaderHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthHeaderHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Получаем токен ДО отправки запроса
            string? token = null;

            // Пробуем получить токен из сессии
            if (_httpContextAccessor.HttpContext != null)
            {
                // Принудительно загружаем сессию, если нужно
                await _httpContextAccessor.HttpContext.Session.LoadAsync(cancellationToken);

                token = _httpContextAccessor.HttpContext.Session.GetString("access_token");

                if (string.IsNullOrEmpty(token))
                {
                    // Пробуем из куки
                    token = _httpContextAccessor.HttpContext.Request.Cookies["access_token"];
                }
            }

            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogDebug($"Adding Bearer token to request: {request.RequestUri}");
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning($"No token found for request to {request.RequestUri}");
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Если получили 401, возможно токен истек
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning($"Request to {request.RequestUri} returned 401 Unauthorized");
                // Не очищаем сессию здесь - это делает AuthService при refresh
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AuthHeaderHandler");
            throw;
        }
    }
}