using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.Endpoints.Comments;

/// <summary>
/// 
/// </summary>
public static class ReadCommentOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddReadCommentOperations(this IEndpointRouteBuilder endpoints)
    {
        AddGetCommentsOperation(endpoints);
        AddGetCommentOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddGetCommentsOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/comments", async (
            [FromServices] ILogger<IApplicationBuilder> logger, 
            [FromServices] ICommentRepository repository) =>
        {
            logger.LogInformation("Start get all comments.");

            try
            {
                var comments = (await repository.GetAllAsync()).ToArray();

                logger.LogInformation("Found {CommentCount} comments.", comments.Length);

                return Results.Ok(comments);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while getting all comments. Operation: {Operation}",
                    "GET /comments"
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
    private static void AddGetCommentOperation(IEndpointRouteBuilder endpoints)
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
                    $"POST /comments/{id}"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}