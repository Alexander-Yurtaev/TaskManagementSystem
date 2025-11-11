using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.ApiEndpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class ReadProjectOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// проектов в API:
    ///   GET /projects            → Получение списка всех проектов;
    ///   GET /projects/{id}       → Получение проекта по идентификатору;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddReadProjectOperations(this IEndpointRouteBuilder endpoints)
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
        })
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Получение списка всех проектов."
        });
    
        return endpoints.MapGet("/projects/{id}", async (
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
        })
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Получение проекта по идентификатору."
        });
    }
}