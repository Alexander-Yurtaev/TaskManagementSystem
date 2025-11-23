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
public static class CreateTaskOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// задач в API:
    ///   POST /tasks       → Создание новой задачи;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddCreateTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tasks", async (
            [FromBody] TaskCreate task,
            HttpContext httpContext,
            IRabbitMqService rabbitMqService,
            ILogger<IApplicationBuilder> logger,
            IMapper mapper,
            ITaskRepository repository) =>
        {
            logger.LogInformation("Start creating task with Title: {Title}.", task.Title);

            var result = await ValidateData(task, repository, logger);
            if (!result.IsValid)
            {
                return ResultHelper.CreateValidationErrorResult(
                    entityName: "Task",
                    entityIdentifier: task.Title,
                    errorMessage: result.ErrorMessage,
                    logger);
            }

            try
            {
                var entity = mapper.Map<TaskEntity>(task);

                await repository.AddAsync(entity);

                var message = new TaskMessage(TaskMessageType.Create, $"Task created: ID = {entity.Id}.");
                await rabbitMqService.SendMessageAsync(message);

                logger.LogInformation("Finish create task with title={TaskTitle}.", task.Title);

                return Results.Created($"api/tasks/{entity.Id}", entity);
            }
            catch (Exception ex)
            {
                return ResultHelper.CreateInternalServerErrorProblemResult(logger, ex);
            }
        })
        .WithName("CreateTask")
        .RequireAuthorization()
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Создание новой задачи."
        })
        .Produces<TaskEntity>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Создание новой задачи.";
            operation.Description = "Создает новую задачу с указанными параметрами.";
            OpenApiMigrationHelper.AddTag(operation, "Task");

            // Настраиваем запрос
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
                                ["projectId"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Идентификатор проекта",
                                    Minimum = 1
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
                            Required = new HashSet<string> { "title", "status", "priority", "projectId" }
                        },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["NewTask"] = new()
                            {
                                Summary = "Новая задача",
                                Value = new OpenApiObject
                                {
                                    ["title"] = new OpenApiString("Разработать API для задач"),
                                    ["description"] = new OpenApiString("Создать REST API для управления задачами проекта"),
                                    ["deadline"] = new OpenApiString("2024-12-31T18:00:00Z"),
                                    ["status"] = new OpenApiInteger(0), // New
                                    ["priority"] = new OpenApiInteger(2), // High
                                    ["projectId"] = new OpenApiInteger(1),
                                    ["assigneeId"] = new OpenApiInteger(123)
                                }
                            },
                            ["SimpleTask"] = new()
                            {
                                Summary = "Простая задача",
                                Value = new OpenApiObject
                                {
                                    ["title"] = new OpenApiString("Обновить документацию"),
                                    ["description"] = new OpenApiString(""),
                                    ["status"] = new OpenApiInteger(0), // New
                                    ["priority"] = new OpenApiInteger(1), // Medium
                                    ["projectId"] = new OpenApiInteger(1)
                                }
                            }
                        }
                    }
                }
            };

            // Настраиваем ответы
            operation.Responses["201"] = new OpenApiResponse
            {
                Description = "Задача успешно создана",
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
                            ["TaskCreated"] = new()
                            {
                                Summary = "Успешное создание задачи",
                                Value = new OpenApiObject
                                {
                                    ["id"] = new OpenApiInteger(1),
                                    ["title"] = new OpenApiString("Разработать API для задач"),
                                    ["description"] = new OpenApiString("Создать REST API для управления задачами проекта"),
                                    ["deadline"] = new OpenApiString("2024-12-31T18:00:00Z"),
                                    ["status"] = new OpenApiInteger(0),
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
                                    ["detail"] = new OpenApiString("Task Title must be set.")
                                }
                            },
                            ["DuplicateTask"] = new()
                            {
                                Summary = "Дублирующаяся задача",
                                Value = new OpenApiObject
                                {
                                    ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                                    ["title"] = new OpenApiString("Bad Request"),
                                    ["status"] = new OpenApiInteger(400),
                                    ["detail"] = new OpenApiString("Task with title='Разработать API' and projectId=1 already exists.")
                                }
                            },
                            ["InvalidProject"] = new()
                            {
                                Summary = "Неверный проект",
                                Value = new OpenApiObject
                                {
                                    ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                                    ["title"] = new OpenApiString("Bad Request"),
                                    ["status"] = new OpenApiInteger(400),
                                    ["detail"] = new OpenApiString("Project ID must be set.")
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

    private static async Task<ValidationResult> ValidateData(TaskCreate task, ITaskRepository repository, ILogger<IApplicationBuilder> logger)
    {
        // Валидация на null
        var taskValidation = CommonValidator.EntityNotNullValidate(task, "Task");
        if (!taskValidation.IsValid)
        {
            return taskValidation;
        }

        var validationResult = TaskValidator.TaskValidate(task);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Task validation failed: {TaskTitle}, Error: {Error}", task.Title, validationResult.ErrorMessage);
            return validationResult;
        }

        var trimmedTitle = task.Title.Trim();
        var taskExists = await repository.IsExistsAsync(trimmedTitle, task.ProjectId);
        if (taskExists)
        {
            return ValidationResult.Error($"Task with title='{task.Title}' and projectId={task.ProjectId} already exists.");
        }

        return ValidationResult.Success();
    }

    #endregion Private Methods
}