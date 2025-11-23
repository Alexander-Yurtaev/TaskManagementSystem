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
using TMS.TaskService.Models.Attachments;
using TMS.TaskService.Services;

namespace TMS.TaskService.Extensions.ApiEndpoints.Attachments;

/// <summary>
/// 
/// </summary>
public static class CreateAttachmentOperations
{
    // Лимит размера файла
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

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
            [FromForm] AttachmentUploadRequest request,
            IConfiguration configuration,
            IFileToStorageService fileToStorageService,
            ILogger <IApplicationBuilder> logger,
            IMapper mapper,
            ITaskRepository taskRepository,
            IAttachmentRepository attachmentRepository,
            IHttpClientFactory httpClientFactory) =>
        {
            logger.LogInformation("For task with id={TaskId} start creating attachment with FileName: {FileName}.", id, request.FileName);

            var result = await ValidateData(id, request.FileName, request.File, configuration, taskRepository, logger);

            if (!result.IsValid)
            {
                return ResultHelper.CreateValidationErrorResult(
                    entityName: "Attachment",
                    entityIdentifier: request.FileName,
                    errorMessage: result.ErrorMessage,
                    logger);
            }

            try
            {
                // Сохранить файл
                string filePath = GetFilePath(id);

                logger.LogInformation("Sending file to FileStorage.");
                var response = await fileToStorageService.SendFileToStorageService(filePath, Guid.NewGuid().ToString(), request.File, httpClientFactory);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    return ResultHelper.CreateExternalServiceErrorResult(
                        serviceName: "FileStorage",
                        operation: "file_upload",
                        statusCode: response.StatusCode,
                        logger: logger,
                        additionalInfo: errorContent.Length > 0 ? errorContent : null);
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
                    FileName = request.FileName,
                    FilePath = fileStorageResult.FilePath
                };
                await attachmentRepository.AddAsync(entity);

                //
                logger.LogInformation("{@FileStorageResult}", fileStorageResult);
                return Results.Created($"api/attachments/{entity.Id}", entity);
            }
            catch (Exception ex)
            {
                return ResultHelper.CreateInternalServerErrorProblemResult($"Error while creating attachment for task with ID {id}", logger, ex);
            }
        })
        .DisableAntiforgery()
        .WithName("AddAttachment")
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
            OpenApiMigrationHelper.AddTag(operation, "Attachment");

            operation.Parameters = new List<OpenApiParameter>
            {
                new()
                {
                    Name = "id",
                    In = ParameterLocation.Path,
                    Required = true,
                    Description = "Идентификатор задачи",
                    Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                }
            };

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
                                ["fileName"] = new()
                                {
                                    Type = "string",
                                    Description = "Имя файла для сохранения",
                                    MinLength = 1,
                                    MaxLength = 255
                                },
                                ["file"] = new()
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description = "Файл для загрузки"
                                }
                            },
                            Required = new HashSet<string> { "fileName", "file" }
                        },
                        Encoding = new Dictionary<string, OpenApiEncoding>
                        {
                            ["fileName"] = new() { ContentType = "text/plain" },
                            ["file"] = new() { ContentType = "application/octet-stream" }
                        },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["DocumentUpload"] = new()
                            {
                                Summary = "Загрузка документа",
                                Description = "Пример загрузки PDF документа",
                                Value = new OpenApiObject
                                {
                                    ["fileName"] = new OpenApiString("project-specification.pdf"),
                                    ["file"] = new OpenApiString("[binary file data]")
                                }
                            },
                            ["ImageUpload"] = new()
                            {
                                Summary = "Загрузка изображения",
                                Description = "Пример загрузки JPEG изображения",
                                Value = new OpenApiObject
                                {
                                    ["fileName"] = new OpenApiString("screenshot.jpg"),
                                    ["file"] = new OpenApiString("[binary file data]")
                                }
                            }
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

            operation.Responses["400"] = new OpenApiResponse
            {
                Description = "Некорректный запрос",
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
                            ["ValidationError"] = new()
                            {
                                Summary = "Ошибка валидации файла",
                                Value = new OpenApiObject
                                {
                                    ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                                    ["title"] = new OpenApiString("Bad Request"),
                                    ["status"] = new OpenApiInteger(400),
                                    ["detail"] = new OpenApiString("File name contains invalid characters")
                                }
                            },
                            ["FileSizeError"] = new()
                            {
                                Summary = "Превышен размер файла",
                                Value = new OpenApiObject
                                {
                                    ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                                    ["title"] = new OpenApiString("Bad Request"),
                                    ["status"] = new OpenApiInteger(400),
                                    ["detail"] = new OpenApiString("File size 15.25MB exceeds maximum allowed size 10MB")
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses["502"] = new OpenApiResponse
            {
                Description = "Ошибка внешнего сервиса",
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
                            ["FileStorageError"] = new()
                            {
                                Summary = "Ошибка файлового хранилища",
                                Value = new OpenApiObject
                                {
                                    ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.6.2"),
                                    ["title"] = new OpenApiString("Bad Gateway"),
                                    ["status"] = new OpenApiInteger(502),
                                    ["detail"] = new OpenApiString("External service 'FileStorage' returned error during file_upload: InternalServerError")
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

            operation = OpenApiSecurityHelper.AddSecurityRequirementHelper(operation);

            return operation;
        });
    }

    #region Private Methods

    private static async Task<ValidationResult> ValidateData(
            int taskId,
            string fileName,
            IFormFile file,
            IConfiguration configuration,
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
        var fileNameValidation = FileNameValidator.ValidateFileName(fileName, configuration);
        if (!fileNameValidation.IsValid)
        {
            logger.LogWarning("File name validation failed: {FileName}, Error: {Error}", fileName, fileNameValidation.ErrorMessage);
            return fileNameValidation;
        }

        // Валидация MIME типа
        var allowedExtensions = configuration.GetSection("AllowedFileExtensions").Get<string[]>();
        var mimeValidation = MimeTypeValidator.ValidateMimeType(file, allowedExtensions);
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

    #endregion Private Methods
}