using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.Common.RabbitMq;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Models.Tasks;

namespace TMS.TaskService.Extensions.ApiEndpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class UpdateTaskOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// задач в API:
    ///   PUT /tasks/{id}            → Обновление задачи по идентификатору;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddUpdateTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/tasks/{id}", async (
            [FromRoute] int id,
            [FromBody] TaskUpdate taskUpdate,
            [FromServices] IRabbitMqService rabbitMqService,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] ITaskRepository repository) =>
        {
            logger.LogInformation("Start updating task with id: {Id}.", id);

            try
            {
                var task = await repository.GetByIdAsync(id);

                if (task is null)
                {
                    logger.LogWarning("Task not found with id: {Id}.", id);

                    return Results.BadRequest($"Task with id={id} does not exists.");
                }

                mapper.Map(taskUpdate, task);

                task = await repository.UpdateAsync(task);

                var message = new TaskMessage(TaskMessageType.Update, $"Task updated: ID = {id}.");
                await rabbitMqService.SendMessageAsync(message);

                return Results.Ok(task);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while updating task with Name: {TaskId}. Operation: {Operation}",
                    id,
                    $"PUT /tasks/{id}"
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
            Summary = "Обновление задачи по идентификатору."
        });
    }
}