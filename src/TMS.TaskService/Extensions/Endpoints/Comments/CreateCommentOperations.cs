using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Comments;

namespace TMS.TaskService.Extensions.Endpoints.Comments;

/// <summary>
/// 
/// </summary>
public static class CreateCommentOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddCreateCommentOperations(this IEndpointRouteBuilder endpoints)
    {
        AddCreateCommentOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddCreateCommentOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/comments", async (
            [FromBody] CommentCreate comment,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] ICommentRepository repository) =>
        {
            logger.LogInformation("Start creating comment with Text: {Text}.", comment.Text.Substring(0, 25));

            try
            {
                var commentExists = await repository.IsExistsAsync(comment.Text);
                if (commentExists)
                {
                    logger.LogError(
                        "Comment with Text='{CommentText}' already exists. Operation: {Operation}",
                        comment.Text.Substring(0, 25),
                        "POST /comments"
                    );

                    return Results.BadRequest($"Comment with Text='{comment.Text.Substring(0, 25)}' already exists.");
                }

                var entity = mapper.Map<CommentEntity>(comment);

                await repository.AddAsync(entity);

                return Results.Created($"api/comments/{entity.Id}", entity);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while creating comment with Text: {CommentText}. Operation: {Operation}",
                    comment.Text.Substring(0, 25),
                    "POST /comments"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}