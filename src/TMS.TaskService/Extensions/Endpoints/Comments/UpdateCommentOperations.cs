using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Models.Comments;

namespace TMS.TaskService.Extensions.Endpoints.Comments;

/// <summary>
/// 
/// </summary>
public static class UpdateCommentOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddUpdateCommentOperations(this IEndpointRouteBuilder endpoints)
    {
        AddUpdateCommentOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddUpdateCommentOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/comments/{id}", async (
            [FromRoute] int id,
            [FromBody] CommentUpdate commentUpdate,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] ICommentRepository repository) =>
        {
            logger.LogInformation("Start updating comment with id: {Id}.", id);

            try
            {
                var comment = await repository.GetByIdAsync(id);

                if (comment is null)
                {
                    logger.LogWarning("Comment not found with id: {Id}.", id);

                    return Results.BadRequest($"Comment with id={id} does not exists.");
                }

                mapper.Map(commentUpdate, comment);

                comment = await repository.UpdateAsync(comment);

                return Results.Ok(comment);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while updating comment with Name: {CommentId}. Operation: {Operation}",
                    id,
                    $"PUT /comments/{id}"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}