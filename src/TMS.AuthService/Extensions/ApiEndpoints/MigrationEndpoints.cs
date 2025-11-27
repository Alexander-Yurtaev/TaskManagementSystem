using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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
    /// <response code="200">
    /// > - Message: результат миграции
    /// > - PendingMigrations: список ожидающих миграций
    /// > - AppliedMigrations: список всех примененных когда-либо миграций
    /// </response>
    /// <response code="500">Ошибка при миграции.</response>
    public static RouteHandlerBuilder AddMigrateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/setup", async (
                HttpContext context,
                [FromServices] AuthDataContext db,
                [FromServices] ILogger<IApplicationBuilder> logger) =>
            {
                if (await DatabaseIsExists(db))
                {
                    return Results.BadRequest("Система уже настроена.");
                }

                return await Migrate(db, logger);
            })
            .WithName("SetupDatabase")
            .AllowAnonymous()
            .WithMetadata(new OpenApiOperation
            {
                Summary = "Запуск начальной настройки БД."
            })
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation = OpenApiMigrationHelper.InitOperationForSetup(operation, "tms-auth-db", "Auth.Setup");
                return operation;
            });

        return endpoints.MapGet("/migrate", async (
                HttpContext context,
                [FromServices] AuthDataContext db,
                [FromServices] ILogger<IApplicationBuilder> logger) =>
        {
            return await Migrate(db, logger);
        })
            .WithName("MigrateDatabase")
            .RequireAuthorization()
            .WithMetadata(new OpenApiOperation
            {
                Summary = "Запуск миграции БД для Сервиса аутентификации."
            })
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation = OpenApiMigrationHelper.InitOperationForMigration(operation, "tms-auth-db", "Auth.Setup");
                operation = OpenApiSecurityHelper.AddSecurityRequirementHelper(operation);
                return operation;
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

    private static async Task<IResult> Migrate(AuthDataContext db, ILogger<IApplicationBuilder> logger)
    {
        var details = await MigrateHelper.Migrate(db, logger);
        if (details.StatusCode == StatusCodes.Status200OK)
        {
            return Results.Ok(details.Result);
        }

        return Results.Problem(detail: details.Detail, statusCode: details.StatusCode);
    }

    #endregion Private Methods
}