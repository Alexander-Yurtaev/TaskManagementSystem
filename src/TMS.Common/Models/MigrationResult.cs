namespace TMS.Common.Models;

public class MigrationResult
{
    public string Message { get; set; } = string.Empty;

    public IEnumerable<string> PendingMigrations { get; set; } = new List<string>();

    public IEnumerable<string> AppliedMigrations { get; set; } = new List<string>();
}