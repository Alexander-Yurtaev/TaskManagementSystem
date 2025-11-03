using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS.AuthService.Data;
using TMS.Common;

namespace TMS.AuthService.Extensions.Endpoints;

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
        var endpoints = (IEndpointRouteBuilder) app;
        endpoints.MapGet("/migrate", async (
                [FromServices] AuthDataContext db,
                [FromServices] ILogger<IApplicationBuilder> logger) =>
        {
            string databaseName;
            
            try
            {
                databaseName = db.Database.GetDbConnection().Database;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while getting database."
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }

            logger.LogInformation("Start migrating for database: {DatabaseName}.", databaseName);

            try
            {
                var result = new MigrationResult
                {
                    PendingMigrations = await db.Database.GetPendingMigrationsAsync()
                };

                await db.Database.MigrateAsync();
                
                result.AppliedMigrations = await db.Database.GetAppliedMigrationsAsync();

                result.Message = $"База данных {databaseName} успешно обновлена!";

                logger.LogInformation("Migrate for database={DatabaseName} finish with result: {@Result}", databaseName, result);

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while migrating database with Name: {DatabaseName}.",
                    databaseName
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .AllowAnonymous();
    }
}
