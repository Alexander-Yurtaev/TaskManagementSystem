using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMS.Common.Models;

namespace TMS.Common.Helpers;

public partial class MigrateHelper
{
    public static async Task<MigrationDetails> Migrate<T>(T db, ILogger logger) where T : DbContext
    {
        string databaseName;

        ArgumentNullException.ThrowIfNull(db, nameof(db));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

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

            return new MigrationDetails(Detail: ex.Message, StatusCode: StatusCodes.Status500InternalServerError);
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

            return new MigrationDetails(Detail: "Success.", StatusCode: StatusCodes.Status200OK) { Result = result };
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while migrating database with Name: {DatabaseName}.",
                databaseName
            );

            return new MigrationDetails(Detail: ex.Message, StatusCode: StatusCodes.Status500InternalServerError);
        }
    }
}