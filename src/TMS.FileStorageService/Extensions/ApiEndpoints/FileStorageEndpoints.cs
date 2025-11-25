using Microsoft.AspNetCore.Mvc;
using TMS.Common.Helpers;
using TMS.Common.Models;
using TMS.FileStorageService.Models;
using TMS.FileStorageService.Services;

namespace TMS.FileStorageService.Extensions.ApiEndpoints;

/// <summary>
/// 
/// </summary>
public static class FileStorageEndpoints
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// файлового хранилища в API:
    ///   GET /files      → получение файла (не объекта) вложения;
    ///   POST /files     → сохранение файла (не объекта) в хранилище;
    /// </summary>
    /// <param name="endpoints"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder AddFileStoragesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/files", async (
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
                var downloadResult = await fileService.DownloadFileAsync(filePath);

                if (!downloadResult.IsSuccess)
                {
                    return Results.BadRequest(downloadResult.Message);
                }

                // ASP.NET Core автоматически закроет поток после отправки клиенту
                return Results.File(downloadResult.Stream, contentType: "application/octet-stream", fileDownloadName: fileName);
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
        })
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Получение файла (не объекта) вложения."
        });

        return endpoints.MapPost("/files", async (
            [FromForm] AttachmentModel attachment,
            IFormFile file,
            [FromKeyedServices("AttachmentFiles")] IFileService fileService,
            [FromServices] ILogger logger) =>
        {
            // 1. Проверка входных данных
            if (file.Length == 0)
                return Results.BadRequest("The file is not provided.");

            if (string.IsNullOrEmpty(attachment.FileName))
                return Results.BadRequest("The file name is not specified.");

            if (string.IsNullOrEmpty(attachment.FilePath))
                return Results.BadRequest("The path to save is not specified.");

            // 2. Формируем полный путь к файлу
            if (!FileHelper.IsPathSafe(attachment.FilePath, attachment.FileName))
            {
                logger.LogWarning("Path traversal attempt detected. Base: {BasePath}, User: {UserPath}",
                                attachment.FilePath, attachment.FileName);
                throw new ArgumentException(nameof(attachment.FilePath));
            }

            string filePath = Path.Combine(attachment.FilePath, attachment.FileName);

            // 3. Сохраняем файл
            try
            {
                var result = await fileService.UploadFileAsync(file);

                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Error saving the file: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .DisableAntiforgery()
        .RequireAuthorization()
        .Produces<FileUploadResult>(StatusCodes.Status200OK)
        .Produces<FileUploadResult>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Сохранение файла (не объекта) в хранилище."
        });
    }
}