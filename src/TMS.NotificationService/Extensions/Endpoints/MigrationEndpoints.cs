using Microsoft.EntityFrameworkCore;
using TMS.Common;
using TMS.NotificationService.Data;

namespace TMS.NotificationService.Extensions.Endpoints;

public static class MigrationEndpoints
{
    public static void AddMigrateEndpoint(this IApplicationBuilder app)
    {
        var endpoints = (IEndpointRouteBuilder) app;
        endpoints.MapGet("/api/migrate", async () =>
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var logger = serviceScope.ServiceProvider.GetService<ILogger<Program>>();
            var context = serviceScope.ServiceProvider.GetService<NotificationDataContext>();

            if (context is null) return Results.InternalServerError("Instance of NotificationDataContext is null!");
            var databaseName = context.Database.GetDbConnection().Database;

            try
            {
                var result = new MigrationResult();
                result.PendingMigrations = await context.Database.GetPendingMigrationsAsync();
                await context.Database.MigrateAsync();
                result.AppliedMigrations = await context.Database.GetAppliedMigrationsAsync();

                result.Message = $"База данных {databaseName} успешно обновлена!";

                logger?.LogCritical(result.Message);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex, $"Ошибка при развертывании миграции для БД {databaseName} для NotificationDataContext!");
                throw;
            }
        });
    }
}
