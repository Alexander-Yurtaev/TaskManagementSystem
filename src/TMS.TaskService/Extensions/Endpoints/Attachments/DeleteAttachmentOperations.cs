using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.Endpoints.Attachments;

/// <summary>
/// 
/// </summary>
public static class DeleteAttachmentOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddDeleteAttachmentOperations(this IEndpointRouteBuilder endpoints)
    {
        AddDeleteAttachmentOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddDeleteAttachmentOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/attachments/{id}", async (
            [FromRoute] int id,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IAttachmentRepository repository) =>
        {
            logger.LogInformation("Start deleting attachment with id: {Id}.", id);

            try
            {
                await repository.DeleteAsync(id);

                logger.LogInformation("Finish delete attachment with id={Id}.", id);

                return Results.NoContent();
            }
            catch (KeyNotFoundException knf)
            {
                logger.LogError(
                    knf,
                    "Attachment not found with Id: {AttachmentId}. Operation: {Operation}",
                    id,
                    "DELETE /attachments"
                );

                return Results.NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while deleting attachment with Id: {AttachmentId}. Operation: {Operation}",
                    id,
                    "DELETE /attachments"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}