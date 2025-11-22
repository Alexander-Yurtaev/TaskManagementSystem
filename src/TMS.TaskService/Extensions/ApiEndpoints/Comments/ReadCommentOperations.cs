using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using TMS.Common.Helpers;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;

namespace TMS.TaskService.Extensions.ApiEndpoints.Comments;

/// <summary>
/// 
/// </summary>
public static class ReadCommentOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// комментариев в API:
    ///   GET /comments/{id}            → Получение комментария по идентификатору;
    ///   GET /tasks/{id}/comments      → Получение всех комментариев, прикрепленных к указанной задаче;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddReadCommentOperations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/comments/{id}", async (
                [FromRoute] int id,
                [FromServices] ILogger<IApplicationBuilder> logger,
                [FromServices] ICommentRepository repository) =>
            {
                if (id <= 0)
                {
                    logger.LogWarning("Invalid comment ID: {Id}", id);
                    return Results.Problem(
                        detail: "Comment ID must be positive",
                        statusCode: StatusCodes.Status400BadRequest);
                }

                logger.LogInformation("Start getting comment with id: {Id}.", id);

                try
                {
                    var comment = await repository.GetByIdAsync(id);

                    if (comment is null)
                    {
                        logger.LogWarning("Comment not found with Id: {Id}.", id);

                        return Results.NotFound();
                    }

                    logger.LogInformation("Comment found with id={Id}.", id);

                    return Results.Ok(comment);
                }
                catch (Exception ex)
                {
                    return ResultHelper.CreateInternalServerErrorProblemResult(logger, ex);
                }
            })
            .WithName("GetCommentById")
            .RequireAuthorization()
            .Produces<CommentEntity>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithMetadata(new
            {
                // Для Swagger/документации
                Summary = "Получение комментария по идентификатору."
            })
            .WithOpenApi(operation =>
            {
                operation.Summary = "Получение комментария по идентификатору.";
                operation.Description = "Возвращает комментарий по указанному идентификатору.";
                OpenApiHelper.AddTag(operation, "Comment");

                // Добавляем параметры
                operation.Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "id",
                        In = ParameterLocation.Path,
                        Required = true,
                        Description = "Идентификатор комментария",
                        Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                    }
                };

                // Настраиваем ответы
                operation.Responses["200"] = new OpenApiResponse
                {
                    Description = "Комментарий успешно найден",
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
                                        Description = "Идентификатор комментария"
                                    },
                                    ["text"] = new()
                                    {
                                        Type = "string",
                                        Description = "Текст комментария"
                                    },
                                    ["userId"] = new()
                                    {
                                        Type = "integer",
                                        Format = "int32",
                                        Description = "Идентификатор пользователя"
                                    },
                                    ["createdAt"] = new()
                                    {
                                        Type = "string",
                                        Format = "date-time",
                                        Description = "Дата и время создания комментария"
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
                                Required = new HashSet<string> { "id", "text", "userId", "createdAt", "taskId" }
                            },
                            Examples = new Dictionary<string, OpenApiExample>
                            {
                                ["GetComment"] = new()
                                {
                                    Summary = "Успешное получение комментария",
                                    Value = new OpenApiObject
                                    {
                                        ["id"] = new OpenApiInteger(1),
                                        ["text"] = new OpenApiString("Это текст комментария к задаче"),
                                        ["userId"] = new OpenApiInteger(123),
                                        ["createdAt"] = new OpenApiString("2024-01-15T10:30:00Z"),
                                        ["taskId"] = new OpenApiInteger(456),
                                        ["task"] = new OpenApiNull()
                                    }
                                }
                            }
                        }
                    }
                };

                operation.Responses["404"] = new OpenApiResponse
                {
                    Description = "Комментарий не найден"
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

                operation = OpenApiHelper.AddSecurityRequirementHelper(operation);

                return operation;
            });

        return endpoints.MapGet("/tasks/{id}/comments", async (
                [FromRoute] int id,
                [FromServices] ILogger<IApplicationBuilder> logger,
                [FromServices] ICommentRepository repository) =>
            {
                if (id <= 0)
                {
                    logger.LogWarning("Invalid comment ID: {Id}", id);
                    return Results.Problem(
                        detail: "Comment ID must be positive",
                        statusCode: StatusCodes.Status400BadRequest);
                }

                logger.LogInformation("Start getting comment by task with id: {TaskId}.", id);

                try
                {
                    var comments = (await repository.GetByTaskIdAsync(id)).ToArray();

                    logger.LogInformation("Found {CommentsCount} by task with id={TaskId}.", comments.Count(), id);

                    return Results.Ok(comments);
                }
                catch (Exception ex)
                {
                    return ResultHelper.CreateInternalServerErrorProblemResult(logger, ex);
                }
            })
            .WithName("GetCommentsByTaskId")
            .RequireAuthorization()
            .Produces<CommentEntity[]>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithMetadata(new
            {
                // Для Swagger/документации
                Summary = "Получение всех комментариев, прикрепленных к указанной задаче."
            })
            .WithOpenApi(operation =>
            {
                operation.Summary = "Получение всех комментариев, прикрепленных к указанной задаче.";
                operation.Description = "Возвращает список всех комментариев, связанных с указанной задачей.";
                OpenApiHelper.AddTag(operation, "Comment");

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
                    Description = "Список комментариев успешно получен",
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
                                            Description = "Идентификатор комментария"
                                        },
                                        ["text"] = new()
                                        {
                                            Type = "string",
                                            Description = "Текст комментария"
                                        },
                                        ["userId"] = new()
                                        {
                                            Type = "integer",
                                            Format = "int32",
                                            Description = "Идентификатор пользователя"
                                        },
                                        ["createdAt"] = new()
                                        {
                                            Type = "string",
                                            Format = "date-time",
                                            Description = "Дата и время создания комментария"
                                        },
                                        ["taskId"] = new()
                                        {
                                            Type = "integer",
                                            Format = "int32",
                                            Description = "Идентификатор задачи"
                                        }
                                    },
                                    Required = new HashSet<string> { "id", "text", "userId", "createdAt", "taskId" }
                                }
                            },
                            Examples = new Dictionary<string, OpenApiExample>
                            {
                                ["GetComments"] = new()
                                {
                                    Summary = "Успешное получение списка комментариев",
                                    Value = new OpenApiArray
                                    {
                                        new OpenApiObject
                                        {
                                            ["id"] = new OpenApiInteger(1),
                                            ["text"] = new OpenApiString("Первый комментарий к задаче"),
                                            ["userId"] = new OpenApiInteger(123),
                                            ["createdAt"] = new OpenApiString("2024-01-15T10:30:00Z"),
                                            ["taskId"] = new OpenApiInteger(456)
                                        },
                                        new OpenApiObject
                                        {
                                            ["id"] = new OpenApiInteger(2),
                                            ["text"] = new OpenApiString("Второй комментарий к задаче"),
                                            ["userId"] = new OpenApiInteger(124),
                                            ["createdAt"] = new OpenApiString("2024-01-15T11:30:00Z"),
                                            ["taskId"] = new OpenApiInteger(456)
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

                operation = OpenApiHelper.AddSecurityRequirementHelper(operation);

                return operation;
            });
    }
}