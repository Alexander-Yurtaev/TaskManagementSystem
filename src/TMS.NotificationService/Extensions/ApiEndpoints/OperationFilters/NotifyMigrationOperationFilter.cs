using TMS.Common.OperationFilters;

namespace TMS.NotificationService.Extensions.ApiEndpoints.OperationFilters;

/// <summary>
/// 
/// </summary>
public class NotifyMigrationOperationFilter : BaseMigrateDatabaseOperationFilter
{
    /// <summary>
    /// 
    /// </summary>
    public override string DatabaseName => "notify_db";
}