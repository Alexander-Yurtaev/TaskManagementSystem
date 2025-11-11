using Microsoft.AspNetCore.Mvc;
using TMS.AuthService.Data;

namespace TMS.AuthService.Extensions.ApiEndpoints;

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
        return endpoints.MapGet("/users", async (
                [FromServices] IUserRepository repository,
                [FromServices] ILogger<IApplicationBuilder> logger) =>
            {
                logger.LogInformation("Start creating all users.");

                try
                {
                    var users = (await repository.GetUsersAsync()).ToArray();

                    logger.LogInformation("Found {ProjectsCount} projects.", users.Length);

                    return Results.Ok(users);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Error while getting users. Operation: {Operation}",
                        "POST /login"
                    );

                    return Results.Problem(
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }
            })
            .WithName("users");
    }
}