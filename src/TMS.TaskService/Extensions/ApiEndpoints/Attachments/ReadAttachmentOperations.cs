using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using TMS.Common.Helpers;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;

namespace TMS.TaskService.Extensions.ApiEndpoints.Attachments;

/// <summary>
/// 
/// </summary>
public static class ReadAttachmentOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// вложений в API:
    ///   GET /attachments/{id}         →    получение вложения по идентификатору;
    ///   GET /tasks/{id}/attachments   →    получение всех вложений, прикрепленных к указанной задаче;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddReadAttachmentOperations(this IEndpointRouteBuilder endpoints)
    {
        // Endpoint для получения вложения по ID
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
                return ResultHelper.CreateInternalServerErrorProblemResult($"Error while getting attachments for task ID={id}", logger, ex);
            }
        })
            .WithName("GetAttachmentById")
            .RequireAuthorization()
            .WithMetadata(new
            {
                Summary = "Получение вложения по идентификатору."
            })
            .Produces<AttachmentEntity>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Получение вложения по идентификатору.";
                operation.Description = "Возвращает метаданные вложения по указанному идентификатору.";
                OpenApiMigrationHelper.AddTag(operation, "Attachment");

                // Добавляем параметры
                operation.Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "id",
                        In = ParameterLocation.Path,
                        Required = true,
                        Description = "Идентификатор вложения",
                        Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                    }
                };

                // Настраиваем ответы
                operation.Responses["200"] = new OpenApiResponse
                {
                    Description = "Вложение успешно найдено",
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
                                ["GetAttachment"] = new()
                                {
                                    Summary = "Успешное получение вложения",
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

                operation.Responses["404"] = new OpenApiResponse
                {
                    Description = "Вложение не найдено"
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
                                        ["detail"] = new OpenApiString("Database connection error")
                                    }
                                }
                            }
                        }
                    }
                };

                operation = OpenApiSecurityHelper.AddSecurityRequirementHelper(operation);

                return operation;
            });

        // Endpoint для получения всех вложений задачи
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
                    "Error while getting attachments for task with Id: {TaskId}.",
                    id
                );

                return ResultHelper.CreateInternalServerErrorProblemResult($"Error while getting attachments for task with ID {id}", logger, ex);
            }
        })
            .WithName("GetAttachmentsByTaskId")
            .RequireAuthorization()
            .WithMetadata(new
            {
                Summary = "Получение всех вложений, прикрепленных к указанной задаче."
            })
            .Produces<List<AttachmentEntity>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Получение всех вложений, прикрепленных к указанной задаче.";
                operation.Description = "Возвращает список всех вложений, связанных с указанной задачей.";
                OpenApiMigrationHelper.AddTag(operation, "Attachment");

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
                    }
                };

                // Настраиваем ответы
                operation.Responses["200"] = new OpenApiResponse
                {
                    Description = "Список вложений успешно получен",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new()
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema
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
                                        }
                                    },
                                    Required = new HashSet<string> { "id", "fileName", "filePath", "taskId" }
                                }
                            },
                            Examples = new Dictionary<string, OpenApiExample>
                            {
                                ["GetAttachments"] = new()
                                {
                                    Summary = "Успешное получение списка вложений",
                                    Value = new OpenApiArray
                                    {
                                        new OpenApiObject
                                        {
                                            ["id"] = new OpenApiInteger(1),
                                            ["fileName"] = new OpenApiString("document.pdf"),
                                            ["filePath"] = new OpenApiString("tasks/123/attachments/guid-string1"),
                                            ["taskId"] = new OpenApiInteger(123)
                                        },
                                        new OpenApiObject
                                        {
                                            ["id"] = new OpenApiInteger(2),
                                            ["fileName"] = new OpenApiString("image.jpg"),
                                            ["filePath"] = new OpenApiString("tasks/123/attachments/guid-string2"),
                                            ["taskId"] = new OpenApiInteger(123)
                                        }
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
                                        ["detail"] = new OpenApiString("Database connection error")
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
}