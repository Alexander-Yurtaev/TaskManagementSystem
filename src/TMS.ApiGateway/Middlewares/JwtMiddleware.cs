using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using TMS.ApiGateway.Grpc.Clients;
using Claim = System.Security.Claims.Claim;

namespace TMS.ApiGateway.Middlewares;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Auth.AuthClient _authClient;

    public JwtMiddleware(RequestDelegate next, GrpcClientFactory factory)
    {
        _next = next;
        _authClient = factory.CreateClient<Auth.AuthClient>("AuthClient");
    }

    public async Task Invoke(HttpContext context)
    {
        // Получаем эндпоинт и его метаданные
        var endpoint = context.GetEndpoint();

        // Проверяем наличие атрибута AllowAnonymous
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await _next(context);
            return;
        }

        try
        {
            var token = context.Request.Headers[HeaderNames.Authorization].FirstOrDefault()?.Split(' ').Last();

            if (token == null)
            {
                await HandleUnauthorizedRequest(context);
                return;
            }

            var response = await _authClient.ValidateTokenAsync(new ValidateTokenRequest{ Token = token });
            if (response is null || !response.IsValidate)
            {
                await HandleUnauthorizedRequest(context);
                return;
            }

            var claims = response.Claims.Select(c => new Claim(c.Type, c.Value));
            var identity = new ClaimsIdentity(claims, response.AuthenticationType);
            context.User = new ClaimsPrincipal(identity);

            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleError(context, ex);
        }
    }

    private async Task HandleUnauthorizedRequest(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { Message = "Unauthorized" });
    }

    private async Task HandleError(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { Message = ex.Message });
    }
}
