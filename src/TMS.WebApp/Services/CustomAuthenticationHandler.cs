using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TMS.WebApp.Services;

public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthService _authService;

    public CustomAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IHttpContextAccessor httpContextAccessor,
        IAuthService authService)
        : base(options, logger, encoder)
    {
        _httpContextAccessor = httpContextAccessor;
        _authService = authService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        this.Logger.LogInformation("!!!!!HandleAuthenticateAsync!!!!!");

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return await Task.FromResult(AuthenticateResult.NoResult());
        }

        // Проверяем аутентификацию через AuthService
        if (!_authService.IsAuthenticated())
        {
            return await Task.FromResult(AuthenticateResult.NoResult());
        }

        // Получаем токен для извлечения claims
        var token = _authService.GetAccessToken();
        if (string.IsNullOrEmpty(token))
        {
            return await Task.FromResult(AuthenticateResult.NoResult());
        }

        // Декодируем JWT токен для получения claims
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            return await Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }

        var jwtToken = handler.ReadJwtToken(token);

        // Создаем claims из JWT токена
        var claims = new List<Claim>();

        // Добавляем стандартные claims из JWT
        foreach (var claim in jwtToken.Claims)
        {
            claims.Add(claim);
        }

        // Добавляем имя пользователя из токена или сессии
        var username = jwtToken.Subject;
        if (!string.IsNullOrEmpty(username))
        {
            claims.Add(new Claim(ClaimTypes.Name, username));
        }

        // Создаем identity и principal
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return await Task.FromResult(AuthenticateResult.Success(ticket));
    }
}