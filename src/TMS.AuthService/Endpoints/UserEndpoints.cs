using TMS.AuthService.Data;

namespace TMS.AuthService.Endpoints;

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
        return endpoints.MapGet("/api/users", async (IUserRepository userRepository) =>
            {
                var users = await userRepository.GetUsersAsync();
                return Results.Ok(users);
            })
            .WithName("users")
            .RequireAuthorization();
    }
}