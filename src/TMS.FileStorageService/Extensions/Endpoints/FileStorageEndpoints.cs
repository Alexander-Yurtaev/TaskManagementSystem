using Microsoft.AspNetCore.Mvc;
using TMS.FileStorageService.Models;

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
        endpoints.MapGet("/files/{id:int}", (int id) =>
        {

        });

        return endpoints.MapPost("/files", async (
            [FromForm] AttachmentModel attachment,
            [FromForm] IFormFile file,
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
            var filePath = Environment.GetEnvironmentVariable("FILES_PATH");
            if (string.IsNullOrEmpty(filePath))
            {
                throw new InvalidOperationException("FILES_PATH does not defined.");
            }

            string fullPath = Path.Combine(filePath, attachment.FilePath, attachment.FileName);

            // 3. Проверяем и создаём каталог, если его нет
            string directory = Path.GetDirectoryName(fullPath)!;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 4. Сохраняем файл
            try
            {
                await using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Results.Ok(new
                {
                    Message = "The file was saved successfully.",
                    FilePath = fullPath,
                    FileName = attachment.FileName,
                    FileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail:$"Error saving the file: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .DisableAntiforgery();
    }
}