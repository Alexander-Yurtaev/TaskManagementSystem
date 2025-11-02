using TMS.AuthService.Data;

namespace TMS.AuthService.Extensions.Endpoints;

/// <summary>
/// 
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder AddUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/users", async (IUserRepository repository) =>
            {
                var users = await repository.GetUsersAsync();
                return Results.Ok(users);
            })
            .WithName("users");
    }
}