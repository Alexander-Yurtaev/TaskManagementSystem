using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Projects;

namespace TMS.TaskService.Extensions.Endpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class CreateProjectOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddCreateProjectOperations(this IEndpointRouteBuilder endpoints)
    {
        AddCreateProjectOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddCreateProjectOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/projects", async (
            [FromBody] ProjectCreate project,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] IProjectRepository repository) =>
        {
            var projectExists = await repository.IsExistsAsync(project.Name);
            if (projectExists)
            {
                return Results.BadRequest($"Project with name='{project.Name}' already exists.");
            }

            var entity = mapper.Map<ProjectEntity>(project);

            await repository.AddAsync(entity);

            return Results.Created($"api/projects/{entity.Id}", entity);
        });
    }
}