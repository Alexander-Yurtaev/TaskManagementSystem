using Microsoft.AspNetCore.Mvc;
using TMS.Common.Helpers;
using TMS.TaskService.Data;

namespace TMS.TaskService.Extensions.ApiEndpoints;

/// <summary>
///
/// </summary>
public static class MigrationEndpoints
{
    /// <summary>
    /// Запуск миграции БД для Сервиса для работы с задачами
    /// </summary>
    /// <param name="app"></param>
    public static void AddMigrateEndpoint(this IApplicationBuilder app)
    {
        var endpoints = (IEndpointRouteBuilder)app;
        endpoints.MapGet("/tasks/migrate", async (
                [FromServices] TaskDataContext db,
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
            Summary = "Запуск миграции БД для Сервис для работы с задачами."
        })
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            OpenApiMigrationHelper.InitOperationForMigration(operation, "tms-task-db", "Task");
            operation = OpenApiSecurityHelper.AddSecurityRequirementHelper(operation);
            return operation;
        });
    }
}