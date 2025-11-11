using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.ApiEndpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class DeleteProjectOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// проектов в API:
    ///   DELETE /projects/{id}            → Удаление проекта по идентификатору;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddDeleteProjectOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/projects/{id}", async (
            [FromRoute] int id,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IProjectRepository repository) =>
        {
            logger.LogInformation("Start deleting project with id: {Id}.", id);

            try
            {
                await repository.DeleteAsync(id);

                logger.LogInformation("Finish delete project with id={Id}.", id);

                return Results.NoContent();
            }
            catch (KeyNotFoundException knf)
            {
                logger.LogError(
                    knf,
                    "Project not found with Id: {ProjectId}. Operation: {Operation}",
                    id,
                    "DELETE /projects"
                );

                return Results.NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while creating project with Id: {ProjectId}. Operation: {Operation}",
                    id,
                    "DELETE /projects"
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
            Summary = "Удаление проекта по идентификатору."
        });
    }
}