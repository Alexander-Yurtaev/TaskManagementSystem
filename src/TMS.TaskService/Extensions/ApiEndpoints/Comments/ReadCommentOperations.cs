using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.ApiEndpoints.Comments;

/// <summary>
/// 
/// </summary>
public static class ReadCommentOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// комментариев в API:
    ///   GET /comments/{id}            → Получение комментария по идентификатору;
    ///   GET /tasks/{id}/comments      → Получение всех комментариев, прикрепленных к указанной задаче;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddReadCommentOperations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/comments/{id}", async (
                [FromRoute] int id,
                [FromServices] ILogger<IApplicationBuilder> logger,
                [FromServices] ICommentRepository repository) =>
            {
                logger.LogInformation("Start getting comment with id: {Id}.", id);

                try
                {
                    var comment = await repository.GetByIdAsync(id);

                    if (comment is null)
                    {
                        logger.LogInformation("Comment not found with Id: {Id}.", id);

                        return Results.NotFound();
                    }

                    logger.LogInformation("Comment found with id={Id}.", id);

                    return Results.Ok(comment);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Error while getting comment with Id: {CommentId}. Operation: {Operation}",
                        id,
                        $"GET /comments/{id}"
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
                Summary = "Получение комментария по идентификатору."
            });

        return endpoints.MapGet("/tasks/{id}/comments", async (
                [FromRoute] int id,
                [FromServices] ILogger<IApplicationBuilder> logger,
                [FromServices] ICommentRepository repository) =>
            {
                logger.LogInformation("Start getting comment by task with id: {TaskId}.", id);

                try
                {
                    var comments = (await repository.GetByTaskIdAsync(id)).ToArray();

                    logger.LogInformation("Found {CommentsCount} by task with id={TaskId}.", comments.Count(), id);

                    return Results.Ok(comments);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Error while getting comments by task with Id: {TaskId}. Operation: {Operation}",
                        id,
                        $"GET /tasks/{id}/comments"
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
                Summary = "Получение всех комментариев, прикрепленных к указанной задаче."
            });
    }
}