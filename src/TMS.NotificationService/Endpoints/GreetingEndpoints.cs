namespace TMS.NotificationService.Endpoints;

public static class GreetingEndpoints
{
    public static RouteHandlerBuilder AddGreetingEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/", () => "Hello from NotificationService!");
    }
}