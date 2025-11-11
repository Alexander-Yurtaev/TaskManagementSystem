using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS.AuthService.Data;
using TMS.Common.Helpers;

namespace TMS.AuthService.Extensions.ApiEndpoints;

/// <summary>
///
/// </summary>
public static class MigrationEndpoints
{
    /// <summary>
    /// Запуск миграции БД для Сервиса Сервис аутентификации.
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddMigrateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/migrate", async (
                HttpContext context,
                [FromServices] AuthDataContext db,
                [FromServices] ILogger<IApplicationBuilder> logger) =>
        {
            if (await DatabaseIsExists(db))
            {
                // если Identity==null или пользователь не авторизован, то выходим
                if (!context.User.Identity?.IsAuthenticated ?? true)
                {
                    return Results.Unauthorized();
                }

                if (!context.User.IsInRole("Admin"))
                {
                    return Results.Forbid();
                }
            }

            var details = await MigrateHelper.Migrate(db, logger);
            if (details.StatusCode == StatusCodes.Status200OK)
            {
                return Results.Ok(details.Result);
            }

            return Results.Problem(detail: details.Detail, statusCode: details.StatusCode);
        })
        .WithName("migrate")
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Запуск миграции БД для Сервиса Сервис аутентификации."
        });
    }

    #region Private Methods

    private static async Task<bool> DatabaseIsExists(DbContext db)
    {
        try
        {
            var am = await db.Database.GetAppliedMigrationsAsync();
            return am.Any();
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion Private Methods
}