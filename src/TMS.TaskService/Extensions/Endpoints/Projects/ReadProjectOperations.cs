using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data;

namespace TMS.TaskService.Extensions.Endpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class ReadProjectOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddReadProjectOperations(this IEndpointRouteBuilder endpoints)
    {
        AddGetProjectsOperation(endpoints);
        AddGetProjectOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddGetProjectsOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/projects", async (
            [FromServices] ILogger<IApplicationBuilder> logger, 
            [FromServices] IProjectRepository repository) =>
        {
            var projects = await repository.GetAllAsync();
            return Results.Ok(projects);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddGetProjectOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/projects/{id}", async (
            [FromRoute] int id,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IProjectRepository repository) =>
        {
            var project = await repository.GetByIdAsync(id);
            return Results.Ok(project);
        });
    }
}