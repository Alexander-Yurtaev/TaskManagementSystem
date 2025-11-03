using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Attachments;

namespace TMS.TaskService.Extensions.Endpoints.Attachments;

/// <summary>
/// 
/// </summary>
public static class CreateAttachmentOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    public static void AddCreateAttachmentOperations(this IEndpointRouteBuilder endpoints)
    {
        AddCreateAttachmentOperation(endpoints);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    private static void AddCreateAttachmentOperation(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/attachments", async (
            [FromBody] AttachmentCreate attachment,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] IAttachmentRepository repository) =>
        {
            logger.LogInformation("Start creating attachment with FileName: {FileName}.", attachment.FileName);

            try
            {
                var attachmentExists = await repository.IsExistsAsync(attachment.FilePath, attachment.FileName, attachment.TaskId);
                if (attachmentExists)
                {
                    logger.LogError(
                        "Attachment with FilePath='{FilePath}', FileName='{FileName}' for TaskId={TaskId}  already exists. Operation: {Operation}",
                        attachment.FilePath,
                        attachment.FileName,
                        attachment.TaskId,
                        "POST /attachments"
                    );

                    return Results.BadRequest(
                        $"Attachment with FilePath='{attachment.FilePath}', FileName='{attachment.FileName}' for TaskId={attachment.TaskId}  already exists.");
                }

                var entity = mapper.Map<AttachmentEntity>(attachment);

                await repository.AddAsync(entity);

                return Results.Created($"api/attachments/{entity.Id}", entity);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while creating attachment with FilePath='{FilePath}', FileName='{FileName}' for TaskId={TaskId}  already exists. Operation: {Operation}",
                    attachment.FilePath,
                    attachment.FileName,
                    attachment.TaskId,
                    "POST /attachments"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        });
    }
}