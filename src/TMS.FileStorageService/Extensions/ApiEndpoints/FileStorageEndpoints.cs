using Microsoft.AspNetCore.Mvc;
using TMS.Common.Models;
using TMS.Common.Services;

namespace TMS.FileStorageService.Extensions.Endpoints;

public static class FileStorageEndpoints
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder AddFileStoragesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/files", (
            [FromQuery] string name,
            [FromQuery] string path,
            [FromKeyedServices("AttachmentFiles")] IFileService fileService,
            [FromServices] ILogger<Program> logger) =>
        {
            var fileName = name;
            var filePath = path;

            // 1. Проверяем входные данные
            if (string.IsNullOrEmpty(fileName))
            {
                return Results.BadRequest("FileName is required.");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                return Results.BadRequest("FilePath is required.");
            }

            // 2. Получаем файл
            try
            {
                Stream fileStream = fileService.GetFile(filePath);

                // ASP.NET Core автоматически закроет поток после отправки клиенту
                return Results.File(fileStream, contentType: "application/octet-stream", fileDownloadName: fileName);
            }
            catch (FileNotFoundException fnf)
            {
                logger.LogError(fnf, "Error reading file: {Path}", Path.Combine(filePath, fileName));
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return Results.Problem(detail: $"Error reading file: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        return endpoints.MapPost("/files", async (
            [FromForm] AttachmentModel attachment,
            [FromForm] IFormFile file,
            [FromKeyedServices("AttachmentFiles")] IFileService fileService,
            [FromServices] ILogger<AttachmentModel> logger) =>
        {
            // 1. Проверка входных данных
            if (file.Length == 0)
                return Results.BadRequest("The file is not provided.");

            if (string.IsNullOrEmpty(attachment.FileName))
                return Results.BadRequest("The file name is not specified.");

            if (string.IsNullOrEmpty(attachment.FilePath))
                return Results.BadRequest("The path to save is not specified.");

            // 2. Формируем полный путь к файлу
            string filePath = Path.Combine(attachment.FilePath, attachment.FileName);

            // 3. Сохраняем файл
            try
            {
                await Task.Run(() =>
                {
                    fileService.WriteFile(filePath, file.CopyTo);
                });

                return Results.Ok(new FileStorageResult(
                    Message: "The file was saved successfully.",
                    FilePath: filePath));
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Error saving the file: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .DisableAntiforgery();
    }
}