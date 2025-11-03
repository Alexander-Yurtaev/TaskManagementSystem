using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.Endpoints.Comments;

/// <summary>
/// 
/// </summary>
public static class DeleteCommentOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddDeleteCommentOperations(this IEndpointRouteBuilder endpoints)
    {
        AddDeleteCommentOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddDeleteCommentOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/comments/{id}", async (
            [FromRoute] int id,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] ICommentRepository repository) =>
        {
            logger.LogInformation("Start deleting comment with id: {Id}.", id);

            try
            {
                await repository.DeleteAsync(id);

                logger.LogInformation("Finish delete comment with id={Id}.", id);

                return Results.NoContent();
            }
            catch (KeyNotFoundException knf)
            {
                logger.LogError(
                    knf,
                    "Comment not found with Id: {CommentId}. Operation: {Operation}",
                    id,
                    "DELETE /comments"
                );

                return Results.NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while deleting comment with Id: {CommentId}. Operation: {Operation}",
                    id,
                    "DELETE /comments"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}