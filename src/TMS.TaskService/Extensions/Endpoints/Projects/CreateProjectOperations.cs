using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;
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
            logger.LogInformation("Start creating project with name: {Name}.", project.Name);

            try
            {
                var projectExists = await repository.IsExistsAsync(project.Name);
                if (projectExists)
                {
                    logger.LogError(
                        "Project with name='{ProjectName}' already exists. Operation: {Operation}", 
                        project.Name,
                        "POST /projects"
                    );

                    return Results.BadRequest($"Project with name='{project.Name}' already exists.");
                }

                var entity = mapper.Map<ProjectEntity>(project);

                await repository.AddAsync(entity);

                logger.LogInformation($"Finish create project with name={project.Name}.");

                return Results.Created($"api/projects/{entity.Id}", entity);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while creating project with Name: {ProjectName}. Operation: {Operation}",
                    project.Name,
                    "POST /projects"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}