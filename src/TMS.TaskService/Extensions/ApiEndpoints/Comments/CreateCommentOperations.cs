using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using TMS.Common.Helpers;
using TMS.Common.Validators;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Comments;

namespace TMS.TaskService.Extensions.ApiEndpoints.Comments;

/// <summary>
/// 
/// </summary>
public static class CreateCommentOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// комментариев в API:
    ///   POST /tasks/{id}/comments      → Создание нового комментария, прикрепленного к указанной задаче;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddCreateCommentOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tasks/{id}/comments", async (
                [FromRoute] int id,
                [FromBody] CommentCreate comment,
                [FromServices] ILogger<IApplicationBuilder> logger,
                [FromServices] IMapper mapper,
                [FromServices] ITaskRepository taskRepository,
                [FromServices] ICommentRepository repository) =>
            {
                logger.LogInformation("For task with Id={TaskId} start creating comment with Text: {Text}.", id,
                    StringHelper.GetStringForLogger(comment.Text));

                var result = await ValidateData(id, comment, taskRepository, logger);
                if (!result.IsValid)
                {
                    return ResultHelper.CreateValidationErrorResult(
                    entityName: "Comments",
                    entityIdentifier: $"TaskId={id}",
                    errorMessage: result.ErrorMessage,
                    logger);
                }

                try
                {
                    var commentExists = await repository.IsExistsAsync(comment.Text);
                    if (commentExists)
                    {
                        return ResultHelper.CreateProblemResult(
                            detail: $"Comment with this text already exists for task {id}",
                            statusCode: StatusCodes.Status400BadRequest,
                            logger);
                    }

                    var entity = mapper.Map<CommentEntity>(comment);

                    entity.TaskId = id;

                    await repository.AddAsync(entity);

                    return Results.Created($"api/tasks/{id}/comments/{entity.Id}", entity);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Error while creating for task with id={TaskId} comment with Text: {CommentText}.",
                        id,
                        StringHelper.GetStringForLogger(comment.Text)
                    );

                    return ResultHelper.CreateInternalServerErrorProblemResult(logger, ex);
                }
            })
            .WithName("AddComment")
            .RequireAuthorization()
            .WithMetadata(new
            {
                // Для Swagger/документации
                Summary = "Создание нового комментария, прикрепленного к указанной задаче."
            })
            .Produces<CommentEntity>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Создание нового комментария, прикрепленного к указанной задаче.";
                operation.Description = "Создает новый комментарий и прикрепляет его к задаче с указанным идентификатором.";
                OpenApiMigrationHelper.AddTag(operation, "Comment");

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

                // Настраиваем запрос на основе реальной модели CommentCreate
                operation.RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new()
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["text"] = new()
                                    {
                                        Type = "string",
                                        Description = "Текст комментария",
                                        MinLength = 1,
                                        MaxLength = 2000
                                    },
                                    ["userId"] = new()
                                    {
                                        Type = "integer",
                                        Format = "int32",
                                        Description = "Идентификатор пользователя"
                                    }
                                },
                                Required = new HashSet<string> { "text", "userId" }
                            },
                            Examples = new Dictionary<string, OpenApiExample>
                            {
                                ["CreateComment"] = new()
                                {
                                    Summary = "Создание комментария",
                                    Value = new OpenApiObject
                                    {
                                        ["text"] = new OpenApiString("Это текст комментария к задаче"),
                                        ["userId"] = new OpenApiInteger(123)
                                    }
                                }
                            }
                        }
                    }
                };

                // Настраиваем ответы
                operation.Responses["201"] = new OpenApiResponse
                {
                    Description = "Комментарий успешно создан",
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
                                ["CommentCreated"] = new()
                                {
                                    Summary = "Успешное создание комментария",
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
                                    Summary = "Ошибка валидации",
                                    Value = new OpenApiObject
                                    {
                                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                                        ["title"] = new OpenApiString("Bad Request"),
                                        ["status"] = new OpenApiInteger(400),
                                        ["detail"] = new OpenApiString("Task with ID 123 does not exist.")
                                    }
                                },
                                ["DuplicateComment"] = new()
                                {
                                    Summary = "Дублирующийся комментарий",
                                    Value = new OpenApiObject
                                    {
                                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                                        ["title"] = new OpenApiString("Bad Request"),
                                        ["status"] = new OpenApiInteger(400),
                                        ["detail"] = new OpenApiString("Comment with this text already exists for task 123")
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

    #region Private Methods

    private static async Task<ValidationResult> ValidateData(int taskId, CommentCreate comment, ITaskRepository taskRepository, ILogger<IApplicationBuilder> logger)
    {
        // Валидация задачи
        var taskValidation = await ValidateTaskAsync(taskId, taskRepository);
        if (!taskValidation.IsValid)
        {
            logger.LogWarning("Task validation failed for task {TaskId}: {Error}", taskId, taskValidation.ErrorMessage);
            return taskValidation;
        }

        // Валидация комментария
        var commentValidation = ValidateComment(comment);
        if (!commentValidation.IsValid)
        {
            logger.LogWarning("Comment validation failed: {Error}", commentValidation.ErrorMessage);
            return commentValidation;
        }

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

    private static ValidationResult ValidateComment(CommentCreate comment)
    {
        if (string.IsNullOrWhiteSpace(comment.Text))
        {
            return ValidationResult.Error("Comment text is required.");
        }

        if (comment.Text.Length > 2000) // Пример ограничения
        {
            return ValidationResult.Error("Comment text is too long.");
        }

        if (comment.UserId <= 0)
        {
            return ValidationResult.Error("User ID must be positive.");
        }

        return ValidationResult.Success();
    }

    #endregion Private Methods
}