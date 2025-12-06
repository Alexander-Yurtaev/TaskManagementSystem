using System.IdentityModel.Tokens.Jwt;
using TMS.WebApp.Services;

namespace TMS.WebApp.Middleware;

public class AutoRefreshTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AutoRefreshTokenMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AutoRefreshTokenMiddleware(
        RequestDelegate next,
        ILogger<AutoRefreshTokenMiddleware> logger,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogDebug("context.Request.Path: {Path}", context.Request.Path);

        // Пропускаем запросы, если не /Migrate
        if (!context.Request.Path.StartsWithSegments("/Admin/Migrate"))
        {
            await _next(context);
            return;
        }
        _logger.LogTrace("Continue!!!");
        try
        {
            // Загружаем сессию если она доступна
            if (context.Session?.IsAvailable == true)
            {
                await context.Session.LoadAsync();
            }

            // Получаем токен (с проверкой доступности сессии)
            var token = context.Session?.IsAvailable == true
                ? context.Session.GetString("access_token")
                : null;

            // Если нет в сессии, пробуем куки
            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Cookies["access_token"];
            }
            _logger.LogTrace("token: {Token}", token);
            if (!string.IsNullOrEmpty(token))
            {
                // Проверяем, истек ли токен
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);

                    // Если токен истек или скоро истечет (менее 5 минут)
                    if (jwtToken.ValidTo < DateTime.UtcNow.AddMinutes(5))
                    {
                        _logger.LogInformation("Token expired or about to expire, attempting refresh");

                        // Используем scope для получения IAuthService
                        using var scope = _scopeFactory.CreateScope();
                        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                        var refreshed = await authService.RefreshTokenAsync();

                        if (!refreshed)
                        {
                            _logger.LogWarning("Failed to refresh token, clearing session");

                            // Очищаем сессию если доступна
                            if (context.Session?.IsAvailable == true)
                            {
                                context.Session.Clear();
                            }

                            // Удаляем куки
                            context.Response.Cookies.Delete("access_token");
                            context.Response.Cookies.Delete("refresh_token");

                            // Если refresh не удался, редиректим на логин
                            if (context.Request.Path != "/Admin/Login" &&
                                !context.Request.Path.StartsWithSegments("/Account/Login"))
                            {
                                context.Response.Redirect("/Admin/Login");
                                return;
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Token refreshed successfully");
                        }
                    }
                }
            }
            else
            {
                _logger.LogTrace("Redirect!!!");
                // Если токенов нигде нет, то преходим на страницу авторизации
                context.Response.Redirect("/Admin/Login");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AutoRefreshTokenMiddleware");
        }

        await _next(context);
    }
}