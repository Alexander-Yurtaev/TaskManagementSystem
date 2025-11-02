namespace TMS.TaskService.Extensions.Endpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class ProjectServiceOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddProjectServiceOperations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.AddCreateProjectOperations();
        endpoints.AddReadProjectOperations();
        endpoints.AddUpdateProjectOperations();
        endpoints.AddDeleteProjectOperations();
    }
}