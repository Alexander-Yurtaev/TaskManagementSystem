using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;
using TMS.ApiGateway.gRpcClients;

namespace TMS.ApiGateway.Middlewares;

public class JwtMiddleware(RequestDelegate next, IAuthClient authClient)
{
    private readonly RequestDelegate _next = next;
    private readonly IAuthClient _authClient = authClient;

    public async Task Invoke(HttpContext context)
    {
        // Получаем эндпоинт и его метаданные
        var endpoint = context.GetEndpoint();

        // Проверяем наличие атрибута AllowAnonymous
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() is not null)
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

            var isValid = await _authClient.ValidateTokenAsync(token);
            if (!isValid)
            {
                await HandleUnauthorizedRequest(context);
                return;
            }

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
