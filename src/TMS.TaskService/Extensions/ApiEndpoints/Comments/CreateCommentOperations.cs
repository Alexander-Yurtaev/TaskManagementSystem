using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.Common.Helpers;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Comments;

namespace TMS.TaskService.Extensions.ApiEndpoints.Comments;

/// <summary>
/// 
/// </summary>
public static class CreateCommentOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// комментариев в API:
    ///   POST /tasks/{id}/comments      → Создание нового комментария, прикрепленного к указанной задаче;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddCreateCommentOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tasks/{id}/comments", async (
                [FromRoute] int id,
                [FromBody] CommentCreate comment,
                [FromServices] ILogger<IApplicationBuilder> logger,
                [FromServices] IMapper mapper,
                [FromServices] ICommentRepository repository) =>
            {
                logger.LogInformation("For task with Id={TaskId} start creating comment with Text: {Text}.", id,
                    StringHelper.GetStringForLogger(comment.Text));

                try
                {
                    var commentExists = await repository.IsExistsAsync(comment.Text);
                    if (commentExists)
                    {
                        logger.LogError(
                            "For task with id={TaskId} comment with Text='{CommentText}' already exists. Operation: {Operation}",
                            id,
                            StringHelper.GetStringForLogger(comment.Text),
                            $"POST /tasks/{id}/comments"
                        );

                        return Results.BadRequest(
                            $"For task with id={id} comment with Text='{comment.Text.Substring(0, 25)}' already exists.");
                    }

                    var entity = mapper.Map<CommentEntity>(comment);

                    entity.TaskId = id;

                    await repository.AddAsync(entity);

                    return Results.Created($"api/tasks/{id}/comments/{entity.Id}", entity);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Error while creating for task with id={TaskId} comment with Text: {CommentText}. Operation: {Operation}",
                        id,
                        StringHelper.GetStringForLogger(comment.Text),
                        $"POST /tasks/{id}/comments"
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
                Summary = "Создание нового комментария, прикрепленного к указанной задаче."
            });
    }
}