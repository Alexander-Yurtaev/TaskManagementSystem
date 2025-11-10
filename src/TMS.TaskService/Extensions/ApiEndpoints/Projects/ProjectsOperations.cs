namespace TMS.TaskService.Extensions.Endpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class ProjectsOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddProjectsOperations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.AddCreateProjectOperations();
        endpoints.AddReadProjectOperations();
        endpoints.AddUpdateProjectOperations();
        endpoints.AddDeleteProjectOperations();
    }
}