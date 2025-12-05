using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;

namespace TMS.WebApp.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthHeaderHandler> _logger;
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly object _lockObject = new();

    public AuthHeaderHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthHeaderHandler> logger,
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory,
        IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _httpClient = httpClientFactory.CreateClient("AuthApi");
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Получаем токен
            var token = await GetTokenAsync(cancellationToken);

            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("Adding Bearer token to request: {RequestUri}", request.RequestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("No token found for request to {RequestUri}", request.RequestUri);
            }

            // Отправляем запрос
            var response = await base.SendAsync(request, cancellationToken);

            // Если получили 401, пытаемся обновить токен и повторить запрос
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Request to {RequestUri} returned 401 Unauthorized. Attempting token refresh...",
                    request.RequestUri);

                // Пытаемся обновить токен
                var newToken = await RefreshTokenAsync(cancellationToken);

                if (!string.IsNullOrEmpty(newToken))
                {
                    _logger.LogInformation("Token refreshed successfully. Retrying request...");

                    // Клонируем оригинальный запрос (HttpRequestMessage нельзя использовать дважды)
                    var retryRequest = await CloneRequestAsync(request);
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                    // Отправляем запрос с новым токеном
                    return await base.SendAsync(retryRequest, cancellationToken);
                }
                else
                {
                    _logger.LogError("Failed to refresh token. User will need to re-authenticate.");

                    // Очищаем сессию, если refresh не удался
                    await ClearInvalidSessionAsync();
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AuthHeaderHandler for request {RequestUri}", request.RequestUri);
            throw;
        }
    }

    private async Task<string?> GetTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_httpContextAccessor.HttpContext == null)
                return null;

            // 1. Проверяем кэш в памяти (предотвращаем множественные обновления)
            if (_cache.TryGetValue<string>("current_access_token", out var cachedToken) &&
                !string.IsNullOrEmpty(cachedToken))
            {
                return cachedToken;
            }

            // 2. Пробуем получить токен из сессии
            await _httpContextAccessor.HttpContext.Session.LoadAsync(cancellationToken);
            var sessionToken = _httpContextAccessor.HttpContext.Session.GetString("access_token");

            if (!string.IsNullOrEmpty(sessionToken))
            {
                // Проверяем, не истек ли токен
                if (!IsTokenExpired(sessionToken))
                {
                    _cache.Set("current_access_token", sessionToken, TimeSpan.FromMinutes(5));
                    return sessionToken;
                }
                else
                {
                    _logger.LogDebug("Token from session is expired");
                }
            }

            // 3. Пробуем из куки
            var cookieToken = _httpContextAccessor.HttpContext.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(cookieToken) && !IsTokenExpired(cookieToken))
            {
                // Сохраняем в сессию для будущих запросов
                _httpContextAccessor.HttpContext.Session.SetString("access_token", cookieToken);
                _cache.Set("current_access_token", cookieToken, TimeSpan.FromMinutes(5));
                return cookieToken;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token");
            return null;
        }
    }

    private async Task<string?> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        // Используем блокировку, чтобы предотвратить множественные одновременные refresh-запросы
        lock (_lockObject)
        {
            var cacheKey = "refresh_in_progress";
            if (_cache.TryGetValue(cacheKey, out bool refreshInProgress) && refreshInProgress)
            {
                _logger.LogDebug("Refresh already in progress, waiting...");
                // Ждем завершения обновления
                SpinWait.SpinUntil(() => !_cache.Get<bool>(cacheKey), 5000);

                // Возвращаем обновленный токен из кэша
                if (_cache.TryGetValue<string>("current_access_token", out var refreshedToken))
                {
                    return refreshedToken;
                }
            }

            _cache.Set(cacheKey, true, TimeSpan.FromSeconds(30));
        }

        try
        {
            if (_httpContextAccessor.HttpContext == null)
                return null;

            // 1. Получаем refresh token
            var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["refresh_token"] ??
                              _httpContextAccessor.HttpContext.Session.GetString("refresh_token");

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("No refresh token found");
                return null;
            }

            // 2. Отправляем запрос на обновление токена
            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh")
            {
                Content = JsonContent.Create(new { refreshToken })
            };

            // Важно: НЕ используем this handler для запроса refresh, чтобы избежать рекурсии
            using var client = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();

            // Копируем куки из текущего контекста
            var cookieHeader = _httpContextAccessor.HttpContext.Request.Headers["Cookie"].ToString();
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                refreshRequest.Headers.Add("Cookie", cookieHeader);
            }

            var refreshResponse = await client.SendAsync(refreshRequest, cancellationToken);

            if (refreshResponse.IsSuccessStatusCode)
            {
                var result = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>(cancellationToken);

                if (!string.IsNullOrEmpty(result?.AccessToken))
                {
                    // Сохраняем новый токен
                    await SaveTokenAsync(result.AccessToken, result.RefreshToken);

                    // Кэшируем новый токен
                    _cache.Set("current_access_token", result.AccessToken, TimeSpan.FromMinutes(5));

                    _logger.LogInformation("Token refreshed successfully");
                    return result.AccessToken;
                }
            }
            else
            {
                _logger.LogError("Refresh request failed with status: {StatusCode}", refreshResponse.StatusCode);

                // Если refresh token недействителен, очищаем сессию
                if (refreshResponse.StatusCode == HttpStatusCode.Unauthorized ||
                    refreshResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    await ClearInvalidSessionAsync();
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return null;
        }
        finally
        {
            _cache.Remove("refresh_in_progress");
        }
    }

    private async Task SaveTokenAsync(string accessToken, string? refreshToken = null)
    {
        if (_httpContextAccessor.HttpContext == null)
            return;

        // Сохраняем в сессию
        _httpContextAccessor.HttpContext.Session.SetString("access_token", accessToken);

        if (!string.IsNullOrEmpty(refreshToken))
        {
            _httpContextAccessor.HttpContext.Session.SetString("refresh_token", refreshToken);

            // Также сохраняем refresh token в куки с более долгим сроком
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7) // Refresh token живет дольше
            });
        }

        // Сохраняем access token в куки (опционально)
        _httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        await _httpContextAccessor.HttpContext.Session.CommitAsync();
    }

    private async Task ClearInvalidSessionAsync()
    {
        try
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Session.Clear();

                // Удаляем куки
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("access_token");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("refresh_token");

                await _httpContextAccessor.HttpContext.Session.CommitAsync();

                _logger.LogInformation("Invalid session cleared");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing session");
        }
    }

    private bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                return true;

            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow.AddSeconds(30); // Добавляем запас 30 секунд
        }
        catch
        {
            return true; // Если не можем прочитать токен, считаем его невалидным
        }
    }

    private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);

        // Копируем содержимое
        if (original.Content != null)
        {
            var content = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);

            // Копируем заголовки контента
            foreach (var header in original.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Копируем заголовки (кроме Authorization, который мы заменим)
        foreach (var header in original.Headers)
        {
            if (header.Key != "Authorization")
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Копируем свойства
        foreach (var prop in original.Properties)
        {
            clone.Properties.Add(prop);
        }

        // Копируем версию
        clone.Version = original.Version;

        return clone;
    }

    private class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}