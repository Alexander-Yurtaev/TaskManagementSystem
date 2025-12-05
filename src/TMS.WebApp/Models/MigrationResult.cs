namespace TMS.WebApp.Models;

public record MigrationResult
{
    public string ServiceName { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    // Дополнительные поля для миграций
    public List<string> AppliedMigrations { get; init; } = new();
    public List<string> PendingMigrations { get; init; } = new();
    public string RawResponse { get; init; } = string.Empty;
}