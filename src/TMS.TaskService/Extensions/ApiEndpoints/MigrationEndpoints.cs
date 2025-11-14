using Microsoft.AspNetCore.Mvc;
using TMS.Common.Helpers;
using TMS.Common.Models;
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
        endpoints.MapGet("/migrate", async (
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
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Запуск миграции БД для Сервис для работы с задачами."
        })
        .Produces<MigrationResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}