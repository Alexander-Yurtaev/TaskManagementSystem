using Microsoft.AspNetCore.Mvc;
using TMS.AuthService.Data;
using TMS.Common.Helpers;

namespace TMS.AuthService.Extensions.ApiEndpoints;

/// <summary>
///
/// </summary>
public static class MigrationEndpoints
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="app"></param>
    public static void AddMigrateEndpoint(this IApplicationBuilder app)
    {
        var endpoints = (IEndpointRouteBuilder)app;
        endpoints.MapGet("/migrate", async (
                [FromServices] AuthDataContext db,
                [FromServices] ILogger<IApplicationBuilder> logger) =>
        {
            var details = await MigrateHelper.Migrate(db, logger);
            if (details.StatusCode == StatusCodes.Status200OK)
            {
                return Results.Ok(details.Result);
            }

            return Results.Problem(detail: details.Detail, statusCode: details.StatusCode);
        })
        .AllowAnonymous();
    }
}