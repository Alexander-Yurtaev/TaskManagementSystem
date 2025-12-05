using System.Net.Http.Headers;

namespace TMS.WebApp.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IAuthService _authService;

    public AuthHeaderHandler(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _authService.GetAccessToken();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Если получили 401, пробуем обновить токен и повторить запрос
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await _authService.RefreshTokenAsync();
            if (refreshed)
            {
                // Обновляем токен и повторяем запрос
                token = _authService.GetAccessToken();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Отменяем предыдущий ответ и отправляем новый запрос
                response.Dispose();
                response = await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }
}