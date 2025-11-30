using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TMS.Common.Extensions;

public static class MigrationExtensions
{
    public static async Task MigrateToAsync(this IApplicationBuilder app, string targetMigration)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
        logger.LogInformation("MigrateToAsync");
        await migrator.MigrateAsync(targetMigration);
    }

    public async static Task RunMigrations<TDbContext>(IConfiguration configuration,
        string connectionString,
        string targetMigration) 
        where TDbContext : DbContext
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            // Создаем экземпляр через конструктор с DbContextOptions<TDbContext>
            var contextInstance = Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options);
            if (contextInstance is not TDbContext context)
            {
                throw new InvalidOperationException($"Failed to create instance of {typeof(TDbContext).Name}");
            }

            await context.Database.MigrateAsync(targetMigration);
            Console.WriteLine($"✅ Migrations for {typeof(TDbContext).Name} and targetMigration='{targetMigration}' applied successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Migration failed for {typeof(TDbContext).Name} and targetMigration='{targetMigration}': {ex.Message}");
            Environment.Exit(1);
        }
    }
}