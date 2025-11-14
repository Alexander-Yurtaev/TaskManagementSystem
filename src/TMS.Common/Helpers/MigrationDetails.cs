using TMS.Common.Models;

namespace TMS.Common.Helpers;

public partial class MigrateHelper
{
    public record MigrationDetails(string Detail, int StatusCode)
    {
        public string Detail { get; init; } = Detail;
        public int StatusCode { get; init; } = StatusCode;
        public MigrationResult Result { get; set; } = null!;
    }
}