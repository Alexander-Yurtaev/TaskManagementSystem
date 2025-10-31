namespace TMS.AuthService.Endpoints;

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
        endpoints.MapGet("/", () => "Hello from AuthService!");
        return endpoints.MapGet("/api/", () => "Hello from API AuthService!");
    }
}