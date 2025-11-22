using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using TMS.Common.Helpers;
using TMS.Common.Models;
using TMS.Common.Validators;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;

namespace TMS.TaskService.Extensions.ApiEndpoints.Attachments;

/// <summary>
/// 
/// </summary>
public static class CreateAttachmentOperations
{
    // Лимит размера файла
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private const string FileStorageClientName = "TMS.FileStorageClient";

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
            IFormFile file,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IMapper mapper,
            [FromServices] ITaskRepository taskRepository,
            [FromServices] IAttachmentRepository attachmentRepository,
            [FromServices] IHttpClientFactory httpClientFactory) =>
        {
            logger.LogInformation("For task with id={TaskId} start creating attachment with FileName: {FileName}.", id, fileName);

            var result = await ValidateData(id, fileName, file, taskRepository, logger);

            if (!result.IsValid)
            {
                return ResultHelper.CreateProblemResult(
                    detail: result.ErrorMessage,
                    statusCode: StatusCodes.Status400BadRequest,
                    logger);
            }

            try
            {
                // Сохранить файл
                string filePath = GetFilePath(id);

                logger.LogInformation("Sending file to FileStorage.");
                var response = await SendFileToStorageService(filePath, Guid.NewGuid().ToString(), file, httpClientFactory);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    logger.LogError("File storage error: {StatusCode}, {ErrorContent}",
                        response.StatusCode, errorContent);

                    return ResultHelper.CreateProblemResult(
                        detail: $"File storage service returned: {response.StatusCode}",
                        statusCode: (int)response.StatusCode,
                        logger);
                }

                var json = await response.Content.ReadAsStringAsync();
                var fileStorageResult = JsonSerializer.Deserialize<FileStorageResult>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (fileStorageResult is null)
                {
                    return ResultHelper.CreateProblemResult(
                        detail: "FileStorageResult should not be null!",
                        statusCode: StatusCodes.Status500InternalServerError,
                        logger);
                }

                // сохраняем entity в БД
                var entity = new AttachmentEntity
                {
                    TaskId = id,
                    FileName = fileName,
                    FilePath = fileStorageResult.FilePath
                };
                await attachmentRepository.AddAsync(entity);

                //
                logger.LogInformation("{@FileStorageResult}", fileStorageResult);
                return Results.Created($"api/attachments/{entity.Id}", entity);
            }
            catch (Exception ex)
            {
                return ResultHelper.CreateProblemResult(
                    detail: "Internal server error",
                    statusCode: StatusCodes.Status500InternalServerError,
                    logger,
                    ex);
            }
        })
        .DisableAntiforgery()
        .WithName("Attachments")
        .RequireAuthorization()
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Добавление файла как вложения к указанной задаче."
        })
        .Produces<AttachmentEntity>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Добавление файла как вложения к указанной задаче.";
            operation.Description = "Загружает файл и прикрепляет его к задаче с указанным идентификатором.";
            OpenApiHelper.AddTag(operation, "Task");

            // Добавляем параметры
            operation.Parameters = new List<OpenApiParameter>
            {
                new()
                {
                    Name = "id",
                    In = ParameterLocation.Path,
                    Required = true,
                    Description = "Идентификатор задачи",
                    Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                },
                new()
                {
                    Name = "fileName",
                    In = ParameterLocation.Query,
                    Required = true,
                    Description = "Имя файла для сохранения",
                    Schema = new OpenApiSchema { Type = "string" }
                }
            };

            // Настраиваем запрос
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new()
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["file"] = new()
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description = "Файл для загрузки"
                                }
                            },
                            Required = new HashSet<string> { "file" }
                        },
                        Encoding = new Dictionary<string, OpenApiEncoding>
                        {
                            ["file"] = new() { ContentType = "application/octet-stream" }
                        }
                    }
                }
            };

            // Настраиваем ответы
            operation.Responses["201"] = new OpenApiResponse
            {
                Description = "Вложение успешно создано",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new()
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["id"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Идентификатор вложения"
                                },
                                ["fileName"] = new()
                                {
                                    Type = "string",
                                    Description = "Имя файла",
                                    MaxLength = 255
                                },
                                ["filePath"] = new()
                                {
                                    Type = "string",
                                    Description = "Путь к файлу в хранилище",
                                    MaxLength = 255
                                },
                                ["taskId"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Идентификатор задачи"
                                },
                                ["task"] = new()
                                {
                                    Nullable = true,
                                    Description = "Связанная задача (может быть null)"
                                }
                            },
                            Required = new HashSet<string> { "id", "fileName", "filePath", "taskId" }
                        },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["AddAttachment"] = new()
                            {
                                Summary = "Успешное добавление вложения",
                                Value = new OpenApiObject
                                {
                                    ["id"] = new OpenApiInteger(1),
                                    ["fileName"] = new OpenApiString("document.pdf"),
                                    ["filePath"] = new OpenApiString("tasks/123/attachments/guid-string"),
                                    ["taskId"] = new OpenApiInteger(123),
                                    ["task"] = new OpenApiNull()
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses["500"] = new OpenApiResponse
            {
                Description = "Внутренняя ошибка сервера",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new()
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["type"] = new() { Type = "string" },
                                ["title"] = new() { Type = "string" },
                                ["status"] = new() { Type = "integer" },
                                ["detail"] = new() { Type = "string" }
                            }
                        },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["ServerError"] = new()
                            {
                                Summary = "Ошибка сервера",
                                Value = new OpenApiObject
                                {
                                    ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.6.1"),
                                    ["title"] = new OpenApiString("An error occurred while processing your request."),
                                    ["status"] = new OpenApiInteger(500),
                                    ["detail"] = new OpenApiString("File storage service unavailable")
                                }
                            }
                        }
                    }
                }
            };

            operation = OpenApiHelper.AddSecurityRequirementHelper(operation);

            return operation;
        });
    }

    #region Private Methods

    private static async Task<ValidationResult> ValidateData(
            int taskId,
            string fileName,
            IFormFile file,
            ITaskRepository taskRepository,
            ILogger logger)
    {
        if (file is null)
        {
            logger.LogWarning("File is null for task {TaskId}", taskId);
            return ValidationResult.Error("File is required.");
        }

        // Валидация задачи
        var taskValidation = await ValidateTaskAsync(taskId, taskRepository);
        if (!taskValidation.IsValid)
        {
            logger.LogWarning("Task validation failed for task {TaskId}: {Error}", taskId, taskValidation.ErrorMessage);
            return taskValidation;
        }

        // Валидация имени файла
        var fileNameValidation = FileNameValidator.ValidateFileName(fileName);
        if (!fileNameValidation.IsValid)
        {
            logger.LogWarning("File name validation failed: {FileName}, Error: {Error}", fileName, fileNameValidation.ErrorMessage);
            return fileNameValidation;
        }

        // Валидация MIME типа
        var mimeValidation = MimeTypeValidator.ValidateMimeType(file);
        if (!mimeValidation.IsValid)
        {
            logger.LogWarning("MIME type validation failed: {FileName}, Error: {Error}", file.FileName, mimeValidation.ErrorMessage);
            return mimeValidation;
        }

        // Валидация размера файла
        var sizeValidation = FileSizeValidator.ValidateFileSize(file, MaxFileSize);
        if (!sizeValidation.IsValid)
        {
            logger.LogWarning("File size validation failed: {FileName}, Size: {Size}, Error: {Error}",
                file.FileName, file.Length, sizeValidation.ErrorMessage);
            return sizeValidation;
        }

        logger.LogInformation("All validations passed for task {TaskId} and file {FileName}", taskId, fileName);
        return ValidationResult.Success();
    }

    private static async Task<ValidationResult> ValidateTaskAsync(int taskId,
        ITaskRepository taskRepository,
        ILogger? logger = null)
    {
        try
        {
            var isExists = await taskRepository.IsExistsAsync(taskId);
            if (!isExists)
            {
                return ValidationResult.Error($"Task with ID {taskId} does not exist.");
            }
            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error validating task existence for task {TaskId}", taskId);
            return ValidationResult.Error("Unable to validate task existence.");
        }
    }

    private static string GetFilePath(int taskId) => $"tasks/{taskId}/attachments";

    private static async Task<HttpResponseMessage> SendFileToStorageService(string filePath,
        [FromForm] string fileName,
        IFormFile file,
        [FromServices] IHttpClientFactory httpClientFactory)
    {
        using var client = httpClientFactory.CreateClient(FileStorageClientName);
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