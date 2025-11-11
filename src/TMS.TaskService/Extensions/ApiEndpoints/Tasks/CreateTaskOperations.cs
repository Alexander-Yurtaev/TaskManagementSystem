using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.Common.RabbitMq;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Tasks;

namespace TMS.TaskService.Extensions.ApiEndpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class CreateTaskOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// задач в API:
    ///   POST /projects/{id}       → Создание новой задачи;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddCreateTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tasks", async (
            [FromBody] TaskCreate task,
            [FromServices] IRabbitMqService rabbitMqService,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] ITaskRepository repository) =>
        {
            logger.LogInformation("Start creating task with Title: {Title}.", task.Title);

            try
            {
                var taskExists = await repository.IsExistsAsync(task.Title);
                if (taskExists)
                {
                    logger.LogError(
                        "Task with title='{TaskTitle}' already exists. Operation: {Operation}",
                        task.Title,
                        "POST /tasks"
                    );

                    return Results.BadRequest($"Task with title='{task.Title}' already exists.");
                }

                var entity = mapper.Map<TaskEntity>(task);

                await repository.AddAsync(entity);

                var message = new TaskMessage(TaskMessageType.Create, $"Task created: ID = {entity.Id}.");
                await rabbitMqService.SendMessageAsync(message);

                return Results.Created($"api/tasks/{entity.Id}", entity);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while creating task with Title: {TaskTitle}. Operation: {Operation}",
                    task.Title,
                    "POST /tasks"
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
            Summary = "Создание новой задачи."
        });
    }
}