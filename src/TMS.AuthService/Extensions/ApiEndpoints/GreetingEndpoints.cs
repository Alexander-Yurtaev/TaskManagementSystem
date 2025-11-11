namespace TMS.AuthService.Extensions.ApiEndpoints;

/// <summary>
/// 
/// </summary>
public static class GreetingEndpoints
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder AddGreetingEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", () => "Hello from API AuthService!");
    }
}