using TMS.ApiGateway.Models;

namespace TMS.ApiGateway.Services;

public interface IMigrationService
{
    Task<List<MigrationResult>> MigrateAllAsync();
    Task<MigrationResult> MigrateServiceAsync(string serviceName);
}
