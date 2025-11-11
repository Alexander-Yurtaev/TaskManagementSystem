using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.ApiEndpoints.Tasks;

/// <summary>
///
/// </summary>
public static class ReadTaskOperations
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddReadTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        AddGetTasksOperation(endpoints);
        AddGetTaskOperation(endpoints);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddGetTasksOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/tasks", [Authorize] async (
            HttpContext context,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] ITaskRepository repository) =>
        {
            logger.LogInformation("Start get all tasks.");

            try
            {
                #region Проверяем, что пользователь аутентифицирован

                if (!context.User.Identity?.IsAuthenticated ?? false)
                {
                    return Results.Unauthorized();
                }

                // Получаем ID пользователя (claim "sub" или "nameid")
                var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Получаем имя пользователя (claim "name")
                var userName = context.User.Identity?.Name;

                #endregion Проверяем, что пользователь аутентифицирован

                var tasks = (await repository.GetAllByUserIdAsync(userId)).ToArray();

                logger.LogInformation("Found {TasksCount} tasks.", tasks.Length);

                return Results.Ok(tasks);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while getting all tasks. Operation: {Operation}",
                    "GET /tasks"
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
    private static void AddGetTaskOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/tasks/{id}", async (
            [FromRoute] int id,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] ITaskRepository repository) =>
        {
            logger.LogInformation("Start getting task with id: {Id}.", id);

            try
            {
                var task = await repository.GetByIdAsync(id);

                if (task is null)
                {
                    logger.LogInformation("Task not found with Id: {Id}.", id);

                    return Results.NotFound();
                }

                logger.LogInformation("Task found with id={Id}.", id);

                return Results.Ok(task);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while getting task with Id: {TaskId}. Operation: {Operation}",
                    id,
                    $"POST /tasks/{id}"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}