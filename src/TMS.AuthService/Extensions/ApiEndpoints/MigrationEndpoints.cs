using Microsoft.AspNetCore.Mvc;
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
        return endpoints.MapPost("/migrate", async (
                HttpContext context,
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
            .WithName("MigrateDatabase")
            //.RequireAuthorization()
            .AllowAnonymous()
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
}