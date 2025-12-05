using TMS.WebApp.Models;

namespace TMS.WebApp.Services;

public interface IMigrationService
{
    Task<List<MigrationResult>> MigrateAllAsync();
    Task<MigrationResult> MigrateServiceAsync(string serviceName);
}
