using TMS.Common.OperationFilters;

namespace TMS.TaskService.Extensions.ApiEndpoints.OperationFilters;

/// <summary>
/// 
/// </summary>
public class TaskMigrationOperationFilter : BaseMigrateDatabaseOperationFilter
{
    /// <summary>
    /// 
    /// </summary>
    public override string DatabaseName => "task_db";
}