using TMS.Common.OperationFilters;

namespace TMS.AuthService.Extensions.ApiEndpoints.OperationFilters;

/// <summary>
/// 
/// </summary>
public class AuthMigrationOperationFilter : BaseMigrateDatabaseOperationFilter
{
    /// <summary>
    /// 
    /// </summary>
    public override string DatabaseName => "auth_db";
}