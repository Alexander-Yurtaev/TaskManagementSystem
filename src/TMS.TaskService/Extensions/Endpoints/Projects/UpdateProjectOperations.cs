using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data;
using TMS.TaskService.Models.Projects;

namespace TMS.TaskService.Extensions.Endpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class UpdateProjectOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddUpdateProjectOperations(this IEndpointRouteBuilder endpoints)
    {
        AddUpdateProjectOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddUpdateProjectOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/projects/{id}", async (
            [FromRoute] int id,
            [FromBody] ProjectUpdate projectUpdate,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] IProjectRepository repository) =>
        {
            var project = await repository.GetByIdAsync(id);

            if (project is null)
            {
                return Results.BadRequest($"Project with id={id} does not exists.");
            }

            mapper.Map(projectUpdate, project);

            project = await repository.UpdateAsync(project);

            return Results.Ok(project);
        });
    }
}