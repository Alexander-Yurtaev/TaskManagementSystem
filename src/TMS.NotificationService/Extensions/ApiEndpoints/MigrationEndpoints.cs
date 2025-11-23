using Microsoft.AspNetCore.Mvc;
using TMS.Common.Helpers;
using TMS.NotificationService.Data;

namespace TMS.NotificationService.Extensions.ApiEndpoints;

/// <summary>
///
/// </summary>
public static class MigrationEndpoints
{
    /// <summary>
    /// Запуск миграции БД для Сервиса рассылки сообщений
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddMigrateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/migrate", async (
                [FromServices] NotificationDataContext db,
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
            .RequireAuthorization()
            .WithMetadata(new
            {
                // Для Swagger/документации
                Summary = "Запуск миграции БД для Сервис рассылки сообщений."
            })
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
             {
                 operation = OpenApiMigrationHelper.InitOperationForMigration(operation, "tms-notify-db", "Notify");
                 operation = OpenApiSecurityHelper.AddSecurityRequirementHelper(operation);
                 return operation;
             });
    }
}