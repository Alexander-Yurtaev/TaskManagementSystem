using Microsoft.AspNetCore.Mvc;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.ApiEndpoints.Attachments;

/// <summary>
/// 
/// </summary>
public static class ReadAttachmentOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// вложений в API:
    ///   POST /tasks/{id}/attachments   →    добавление файла как вложения к указанной задаче;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddReadAttachmentOperations(this IEndpointRouteBuilder endpoints)
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
                        $"GET /attachments/{id}"
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
                Summary = "Получение вложения по идентификатору."
            });

        return endpoints.MapGet("/tasks/{id}/attachments", async (
                [FromRoute] int id,
                [FromServices] ILogger<IApplicationBuilder> logger,
                [FromServices] IAttachmentRepository repository) =>
            {
                logger.LogInformation("Start getting Attachment for task with id: {TaskId}.", id);

                try
                {
                    var attachments = (await repository.GetByTaskIdAsync(id)).ToArray();

                    logger.LogInformation("Found {AttachmentsCount} attachments.", attachments.Count());

                    return Results.Ok(attachments);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Error while getting attachments for task with Id: {TaskId}. Operation: {Operation}",
                        id,
                        $"GET /tasks/{id}/attachments"
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
                Summary = "Получение всех вложений, прикрепленных к указанной задаче."
            });
    }
}