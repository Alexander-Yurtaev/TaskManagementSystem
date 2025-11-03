using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.Endpoints.Attachments;

/// <summary>
/// 
/// </summary>
public static class ReadAttachmentOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddReadAttachmentOperations(this IEndpointRouteBuilder endpoints)
    {
        AddGetAttachmentsOperation(endpoints);
        AddGetAttachmentOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddGetAttachmentsOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/attachments", async (
            [FromServices] ILogger<IApplicationBuilder> logger, 
            [FromServices] IAttachmentRepository repository) =>
        {
            logger.LogInformation("Start get all attachments.");

            try
            {
                var attachments = (await repository.GetAllAsync()).ToArray();

                logger.LogInformation("Found {AttachmentCount} attachments.", attachments.Length);

                return Results.Ok(attachments);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while getting all attachments. Operation: {Operation}",
                    "GET /attachments"
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
    private static void AddGetAttachmentOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/attachments/{id}", async (
            [FromRoute] int id,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IAttachmentRepository repository) =>
        {
            logger.LogInformation("Start getting Attachment with id: {Id}.", id);

            try
            {
                var attachment = await repository.GetByIdAsync(id);

                if (attachment is null)
                {
                    logger.LogInformation("Attachment not found with Id: {Id}.", id);

                    return Results.NotFound();
                }

                logger.LogInformation("Attachment found with id={Id}.", id);

                return Results.Ok(attachment);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while getting attachment with Id: {AttachmentId}. Operation: {Operation}",
                    id,
                    $"POST /attachments/{id}"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}