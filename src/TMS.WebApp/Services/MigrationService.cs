using System.Text.Json;
using TMS.WebApp.Models;

namespace TMS.WebApp.Services;

public class MigrationService : IMigrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MigrationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<MigrationResult>> MigrateAllAsync()
    {
        var services = GetMigratableServices();
        var results = new List<MigrationResult>();

        foreach (var service in services)
        {
            var result = await MigrateServiceAsync(service.ServiceName);
            results.Add(result);
        }

        return results;
    }

    public async Task<MigrationResult> MigrateServiceAsync(string serviceName)
    {
        try
        {
            var services = GetMigratableServices();
            var service = services.FirstOrDefault(s =>
                s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

            if (service == null)
            {
                return new MigrationResult
                {
                    ServiceName = serviceName,
                    Success = false,
                    Message = $"Service '{serviceName}' not found in migratable services"
                };
            }

            var client = _httpClientFactory.CreateClient("AuthenticatedClient");

            var response = await client.PostAsync($"{service.BaseUrl}/migrate", null);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var migrationResponse = ParseMigrationResponse(jsonResponse);

                return new MigrationResult
                {
                    ServiceName = serviceName,
                    Success = true,
                    Message = migrationResponse.Message,
                    AppliedMigrations = migrationResponse.AppliedMigrations,
                    PendingMigrations = migrationResponse.PendingMigrations,
                    RawResponse = jsonResponse
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new MigrationResult
                {
                    ServiceName = serviceName,
                    Success = false,
                    Message = $"Failed with status: {response.StatusCode}",
                    RawResponse = errorContent
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration failed for service {ServiceName}", serviceName);
            return new MigrationResult
            {
                ServiceName = serviceName,
                Success = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    private MigrationResponse ParseMigrationResponse(string jsonResponse)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(jsonResponse);
            var root = jsonDoc.RootElement;

            var message = root.GetProperty("message").GetString() ?? "Migration completed";

            var appliedMigrations = new List<string>();
            if (root.TryGetProperty("appliedMigrations", out var appliedElement))
            {
                appliedMigrations = appliedElement.EnumerateArray()
                    .Select(m => m.GetString() ?? string.Empty)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();
            }

            var pendingMigrations = new List<string>();
            if (root.TryGetProperty("pendingMigrations", out var pendingElement))
            {
                pendingMigrations = pendingElement.EnumerateArray()
                    .Select(m => m.GetString() ?? string.Empty)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();
            }

            return new MigrationResponse
            {
                Message = message,
                AppliedMigrations = appliedMigrations,
                PendingMigrations = pendingMigrations
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse migration response");
            return new MigrationResponse
            {
                Message = jsonResponse,
                AppliedMigrations = new List<string>(),
                PendingMigrations = new List<string>()
            };
        }
    }

    private List<MigratorServiceInfo> GetMigratableServices()
    {
        var services = new List<MigratorServiceInfo>();
        var section = _configuration.GetSection("MigratableServices");

        foreach (var child in section.GetChildren())
        {
            services.Add(new MigratorServiceInfo(
                ServiceName: child.Key,
                BaseUrl: child.Value!));
        }

        return services;
    }

    private class MigrationResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<string> AppliedMigrations { get; set; } = new();
        public List<string> PendingMigrations { get; set; } = new();
    }
}