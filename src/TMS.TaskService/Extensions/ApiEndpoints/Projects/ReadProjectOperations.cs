using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.ApiEndpoints.Projects;

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
            logger.LogInformation("Start get all projects.");

            try
            {
                var projects = (await repository.GetAllAsync()).ToArray();

                logger.LogInformation("Found {ProjectsCount} projects.", projects.Length);

                return Results.Ok(projects);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while getting all projects. Operation: {Operation}",
                    "GET /projects"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
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
            logger.LogInformation("Start getting project with id: {Id}.", id);

            try
            {
                var project = await repository.GetByIdAsync(id);

                if (project is null)
                {
                    logger.LogInformation("Project not found with Id: {Id}.", id);

                    return Results.NotFound();
                }

                logger.LogInformation("Project found with id={Id}.", id);

                return Results.Ok(project);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while getting project with Id: {ProjectId}. Operation: {Operation}",
                    id,
                    $"POST /projects/{id}"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}