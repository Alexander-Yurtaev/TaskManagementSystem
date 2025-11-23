using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using TMS.Common.Helpers;
using TMS.Common.RabbitMq;
using TMS.Common.Validators;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Tasks;

namespace TMS.TaskService.Extensions.ApiEndpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class UpdateTaskOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// задач в API:
    ///   PUT /tasks/{id}            → Обновление задачи по идентификатору;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddUpdateTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/tasks/{id}", async (
            [FromRoute] int id,
            [FromBody] TaskUpdate taskUpdate,
            HttpContext httpContext,
            IRabbitMqService rabbitMqService,
            ILogger<IApplicationBuilder> logger,
            IMapper mapper,
            ITaskRepository repository) =>
        {
            logger.LogInformation("Start updating task with id: {Id}.", id);

            // Валидация данных
            var validationResult = await ValidateData(id, taskUpdate, repository, logger);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Task validation failed for task {TaskId}: {Error}", id, validationResult.ErrorMessage);
                return Results.BadRequest(validationResult.ErrorMessage);
            }

            try
            {
                var task = await repository.GetByIdAsync(id);

                if (task is null)
                {
                    logger.LogWarning("Task not found with id: {Id}.", id);
                    return Results.NotFound($"Task with id={id} does not exist.");
                }

                mapper.Map(taskUpdate, task);

                task = await repository.UpdateAsync(task);

                var message = new TaskMessage(TaskMessageType.Update, $"Task updated: ID = {id}.");
                await rabbitMqService.SendMessageAsync(message);

                logger.LogInformation("Task updated successfully with id: {Id}.", id);

                return Results.Ok(task);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while updating task with Id: {TaskId}. Operation: {Operation}",
                    id,
                    $"PUT /tasks/{id}"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .WithName("UpdateTask")
        .RequireAuthorization()
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Обновление задачи по идентификатору."
        })
        .Produces<TaskEntity>(StatusCodes.Status200OK)
        .Produces<string>(StatusCodes.Status400BadRequest)
        .Produces<string>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Обновление задачи по идентификатору.";
            operation.Description = "Обновляет задачу с указанным идентификатором.";
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
                }
            };

            // Описание тела запроса
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
                                ["title"] = new()
                                {
                                    Type = "string",
                                    Description = "Название задачи",
                                    MinLength = 1,
                                    MaxLength = 50
                                },
                                ["description"] = new()
                                {
                                    Type = "string",
                                    Description = "Описание задачи",
                                    MaxLength = 500,
                                    Nullable = true
                                },
                                ["deadline"] = new()
                                {
                                    Type = "string",
                                    Format = "date-time",
                                    Description = "Срок выполнения задачи",
                                    Nullable = true
                                },
                                ["status"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Статус задачи",
                                    Enum = new List<IOpenApiAny>
                                    {
                                        new OpenApiInteger(0), // New
                                        new OpenApiInteger(1), // InProgress
                                        new OpenApiInteger(2), // Completed
                                        new OpenApiInteger(3), // OnHold
                                        new OpenApiInteger(4)  // Cancelled
                                    }
                                },
                                ["priority"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Приоритет задачи",
                                    Enum = new List<IOpenApiAny>
                                    {
                                        new OpenApiInteger(0), // Low
                                        new OpenApiInteger(1), // Medium
                                        new OpenApiInteger(2), // High
                                        new OpenApiInteger(3)  // Critical
                                    }
                                },
                                ["assigneeId"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Идентификатор исполнителя",
                                    Minimum = 1,
                                    Nullable = true
                                }
                            },
                            Required = new HashSet<string> { }
                        },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["UpdateTask"] = new()
                            {
                                Summary = "Обновление задачи",
                                Value = new OpenApiObject
                                {
                                    ["title"] = new OpenApiString("Обновленное название задачи"),
                                    ["description"] = new OpenApiString("Обновленное описание задачи"),
                                    ["deadline"] = new OpenApiString("2024-12-31T18:00:00Z"),
                                    ["status"] = new OpenApiInteger(1), // InProgress
                                    ["priority"] = new OpenApiInteger(2), // High
                                    ["assigneeId"] = new OpenApiInteger(123)
                                }
                            },
                            ["PartialUpdate"] = new()
                            {
                                Summary = "Частичное обновление",
                                Value = new OpenApiObject
                                {
                                    ["title"] = new OpenApiString("Новое название задачи"),
                                    ["status"] = new OpenApiInteger(2) // Completed
                                }
                            }
                        }
                    }
                }
            };

            // Настраиваем ответы
            operation.Responses["200"] = new OpenApiResponse
            {
                Description = "Задача успешно обновлена",
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
                                    Description = "Идентификатор задачи"
                                },
                                ["title"] = new()
                                {
                                    Type = "string",
                                    Description = "Название задачи",
                                    MaxLength = 50
                                },
                                ["description"] = new()
                                {
                                    Type = "string",
                                    Description = "Описание задачи",
                                    MaxLength = 500,
                                    Nullable = true
                                },
                                ["deadline"] = new()
                                {
                                    Type = "string",
                                    Format = "date-time",
                                    Description = "Срок выполнения задачи",
                                    Nullable = true
                                },
                                ["status"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Статус задачи"
                                },
                                ["priority"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Приоритет задачи"
                                },
                                ["projectId"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Идентификатор проекта"
                                },
                                ["assigneeId"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Идентификатор исполнителя",
                                    Nullable = true
                                },
                                ["project"] = new()
                                {
                                    Type = "object",
                                    Description = "Проект задачи",
                                    Nullable = true,
                                    Properties = new Dictionary<string, OpenApiSchema>
                                    {
                                        ["id"] = new() { Type = "integer", Format = "int32" },
                                        ["name"] = new() { Type = "string" }
                                    }
                                },
                                ["comments"] = new()
                                {
                                    Type = "array",
                                    Description = "Комментарии к задаче",
                                    Items = new OpenApiSchema { Type = "object" },
                                    Nullable = true
                                },
                                ["attachments"] = new()
                                {
                                    Type = "array",
                                    Description = "Вложения задачи",
                                    Items = new OpenApiSchema { Type = "object" },
                                    Nullable = true
                                }
                            },
                            Required = new HashSet<string> { "id", "title", "status", "priority", "projectId" }
                        },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["UpdatedTask"] = new()
                            {
                                Summary = "Успешно обновленная задача",
                                Value = new OpenApiObject
                                {
                                    ["id"] = new OpenApiInteger(1),
                                    ["title"] = new OpenApiString("Обновленное название задачи"),
                                    ["description"] = new OpenApiString("Обновленное описание задачи"),
                                    ["deadline"] = new OpenApiString("2024-12-31T18:00:00Z"),
                                    ["status"] = new OpenApiInteger(1),
                                    ["priority"] = new OpenApiInteger(2),
                                    ["projectId"] = new OpenApiInteger(1),
                                    ["assigneeId"] = new OpenApiInteger(123),
                                    ["project"] = new OpenApiNull(),
                                    ["comments"] = new OpenApiArray(),
                                    ["attachments"] = new OpenApiArray()
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
                        Schema = new OpenApiSchema { Type = "string" },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["ValidationError"] = new()
                            {
                                Summary = "Ошибка валидации",
                                Value = new OpenApiString("Task Title must be set.")
                            },
                            ["InvalidData"] = new()
                            {
                                Summary = "Неверные данные",
                                Value = new OpenApiString("Task with ID 123 does not exist.")
                            }
                        }
                    }
                }
            };

            operation.Responses["404"] = new OpenApiResponse
            {
                Description = "Задача не найдена",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new()
                    {
                        Schema = new OpenApiSchema { Type = "string" },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["NotFound"] = new()
                            {
                                Summary = "Задача не найдена",
                                Value = new OpenApiString("Task with id=1 does not exist.")
                            }
                        }
                    }
                }
            };

            operation.Responses["401"] = new OpenApiResponse
            {
                Description = "Пользователь не авторизован"
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

    #region Private Methods

    private static async Task<ValidationResult> ValidateData(
            int taskId,
            TaskUpdate taskUpdate,
            ITaskRepository repository,
            ILogger logger)
    {
        // Валидация существования задачи
        var taskValidation = await ValidateTaskAsync(taskId, repository, logger);
        if (!taskValidation.IsValid)
        {
            return taskValidation;
        }

        // Валидация на null
        taskValidation = CommonValidator.EntityNotNullValidate(taskUpdate, "Task");
        if (!taskValidation.IsValid)
        {
            return taskValidation;
        }

        // Валидация данных задачи
        var validationResult = TaskValidator.TaskValidate(taskUpdate);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        logger.LogInformation("All validations passed for task {TaskId}", taskId);
        return ValidationResult.Success();
    }

    private static async Task<ValidationResult> ValidateTaskAsync(
        int taskId,
        ITaskRepository taskRepository,
        ILogger logger)
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
            logger.LogError(ex, "Error validating task existence for task {TaskId}", taskId);
            return ValidationResult.Error("Unable to validate task existence.");
        }
    }

    #endregion Private Methods
}