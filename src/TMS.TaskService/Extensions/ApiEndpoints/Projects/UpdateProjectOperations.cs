using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Models.Projects;

namespace TMS.TaskService.Extensions.ApiEndpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class UpdateProjectOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// проектов в API:
    ///   PUT /projects/{id}       → Обновление проекта по идентификатору;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddUpdateProjectOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/projects/{id}", async (
            [FromRoute] int id,
            [FromBody] ProjectUpdate projectUpdate,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] IProjectRepository repository) =>
        {
            logger.LogInformation("Start updating project with id: {Id}.", id);

            try
            {
                var project = await repository.GetByIdAsync(id);

                if (project is null)
                {
                    logger.LogWarning("Project not found with id: {Id}.", id);

                    return Results.BadRequest($"Project with id={id} does not exists.");
                }

                mapper.Map(projectUpdate, project);

                project = await repository.UpdateAsync(project);

                return Results.Ok(project);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while updating project with Name: {ProjectId}. Operation: {Operation}",
                    id,
                    $"PUT /projects/{id}"
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
            Summary = "Обновление проекта по идентификатору."
        });
    }
}