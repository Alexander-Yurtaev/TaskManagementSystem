namespace TMS.NotificationService.Extensions.ApiEndpoints;

public static class GreetingEndpoints
{
    public static RouteHandlerBuilder AddGreetingEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/greeting", () => "Hello from NotificationService!");
    }
}