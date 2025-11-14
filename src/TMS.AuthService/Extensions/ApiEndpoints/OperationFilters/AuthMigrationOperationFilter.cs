namespace TMS.AuthService.Extensions.ApiEndpoints.OperationFilters;

/// <summary>
/// 
/// </summary>
public class AuthMigrationOperationFilter : BaseMigrationOperationFilter
{
    /// <summary>
    /// 
    /// </summary>
    public override string DatabaseName => "auth_db";
}