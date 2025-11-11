using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TMS.Common.Models;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;

namespace TMS.TaskService.Extensions.ApiEndpoints.Attachments;

/// <summary>
/// 
/// </summary>
public static class CreateAttachmentOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// вложений в API:
    ///   POST /tasks/{id}/attachments   →    добавление файла как вложения к указанной задаче;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddCreateAttachmentOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tasks/{id}/attachments", async (
            [FromRoute] int id,
            [FromQuery] string fileName,
            [FromForm] IFormFile file,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] IAttachmentRepository repository,
            [FromServices] IHttpClientFactory httpClientFactory) =>
        {
            logger.LogInformation("For task with id={TaskId} start creating attachment with FileName: {FileName}.", id, fileName);

            try
            {
                // Сохранить файл
                string filePath = GetFilePath(id);

                logger.LogInformation("Sending file to FileStorage.");
                var response = await SendFileToStorageService(filePath, Guid.NewGuid().ToString(), file, httpClientFactory);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    logger.LogInformation($"File was not saved: {errorContent}");

                    return Results.Problem(
                        detail: errorContent,
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }

                var json = await response.Content.ReadAsStringAsync();
                var fileStorageResult = JsonSerializer.Deserialize<FileStorageResult>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (fileStorageResult is null)
                {
                    return Results.Problem(
                        detail: "FileStorageResult should not be null!",
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }

                // сохраняем entity в БД
                var entity = new AttachmentEntity
                {
                    TaskId = id,
                    FileName = fileName,
                    FilePath = fileStorageResult.FilePath
                };
                await repository.AddAsync(entity);

                //
                logger.LogInformation("{@FileStorageResult}", fileStorageResult);
                return Results.Created($"api/attachments/{entity.Id}", entity);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while creating attachment for TaskId={TaskId}. Operation: {Operation}",
                    id,
                    $"POST /tasks/{id}/attachments"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .DisableAntiforgery()
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Добавление файла как вложения к указанной задаче."
        });
    }

    #region Private Methods

    private static string GetFilePath(int taskId) => $"tasks/{taskId}/attachments";

    private static async Task<HttpResponseMessage> SendFileToStorageService(string filePath,
        [FromForm] string fileName,
        [FromForm] IFormFile file,
        [FromServices] IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient("TMS.FileStorageClient");
        using var content = new MultipartFormDataContent();

        // 1. Добавляем метаданные
        content.Add(new StringContent(fileName), "FileName");
        content.Add(new StringContent(filePath), "FilePath");

        // 2. Добавляем файл
        await using var fileStream = file.OpenReadStream();
        content.Add(
            new StreamContent(fileStream),
            "File",
            file.FileName
        );

        // 3. Отправляем запрос
        var response = await client.PostAsync(
            "",
            content
        );

        return response;
    }

    #endregion Private Methods
}