using Microsoft.AspNetCore.Mvc;
using TMS.Common.RabbitMq;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.ApiEndpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class DeleteTaskOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// задач в API:
    ///   DELETE /tasks/{id}       → Удаление задачи по идентификатору;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddDeleteTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/tasks/{id}", async (
            [FromRoute] int id,
            [FromServices] IRabbitMqService rabbitMqService,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] ITaskRepository repository) =>
        {
            logger.LogInformation("Start deleting task with id: {Id}.", id);

            try
            {
                await repository.DeleteAsync(id);

                var message = new TaskMessage(TaskMessageType.Delete, $"Task deleted: ID = {id}.");
                await rabbitMqService.SendMessageAsync(message);

                logger.LogInformation("Finish delete task with id={Id}.", id);

                return Results.NoContent();
            }
            catch (KeyNotFoundException knf)
            {
                logger.LogError(
                    knf,
                    "Task not found with Id: {TaskId}. Operation: {Operation}",
                    id,
                    "DELETE /tasks"
                );

                return Results.NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while deleting task with Id: {TaskId}. Operation: {Operation}",
                    id,
                    "DELETE /tasks"
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
            Summary = "Удаление задачи по идентификатору."
        });
    }
}