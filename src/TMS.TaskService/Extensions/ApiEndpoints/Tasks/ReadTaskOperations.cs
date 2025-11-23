using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using TMS.Common.Helpers;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;

namespace TMS.TaskService.Extensions.ApiEndpoints.Tasks;

/// <summary>
///
/// </summary>
public static class ReadTaskOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// задач в API:
    ///   GET /tasks            → Получение списка задач текущего пользователя;
    ///   GET /tasks/{id}       → Получение задачи по идентификатору;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddReadTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/tasks", [Authorize] async (
            HttpContext httpContext,
            ILogger<IApplicationBuilder> logger,
            ITaskRepository repository) =>
        {
            logger.LogInformation("Start get all tasks.");

            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    logger.LogWarning("User ID not found in claims");
                    return Results.Unauthorized();
                }

                var tasks = (await repository.GetAllByUserIdAsync(currentUserId)).ToArray();

                logger.LogInformation("Found {TasksCount} tasks.", tasks.Length);

                return Results.Ok(tasks);
            }
            catch (Exception ex)
            {
                return ResultHelper.CreateInternalServerErrorProblemResult(logger, ex);
            }
        })
        .WithName("GetTasks")
        .RequireAuthorization()
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Получение списка задач текущего пользователя."
        })
        .Produces<TaskEntity[]>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Получение всех задач, прикрепленных к текущему пользователю.";
            operation.Description = "Возвращает список всех задач, связанных с текущим пользователем.";
            OpenApiHelper.AddTag(operation, "Task");

            // Настраиваем ответы
            operation.Responses["200"] = new OpenApiResponse
            {
                Description = "Список задач успешно получен",
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
                                    }
                                },
                                Required = new HashSet<string> { "id", "title", "status", "priority", "projectId" }
                            }
                        },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["GetTasks"] = new()
                            {
                                Summary = "Успешное получение списка задач",
                                Value = new OpenApiArray
                                {
                                    new OpenApiObject
                                    {
                                        ["id"] = new OpenApiInteger(1),
                                        ["title"] = new OpenApiString("Разработать API"),
                                        ["description"] = new OpenApiString("Создать REST API для проекта"),
                                        ["deadline"] = new OpenApiString("2024-12-31T18:00:00Z"),
                                        ["status"] = new OpenApiInteger(0),
                                        ["priority"] = new OpenApiInteger(2),
                                        ["projectId"] = new OpenApiInteger(1),
                                        ["assigneeId"] = new OpenApiInteger(123)
                                    },
                                    new OpenApiObject
                                    {
                                        ["id"] = new OpenApiInteger(2),
                                        ["title"] = new OpenApiString("Написать документацию"),
                                        ["description"] = new OpenApiString("Документирование API"),
                                        ["deadline"] = new OpenApiString("2024-12-15T18:00:00Z"),
                                        ["status"] = new OpenApiInteger(1),
                                        ["priority"] = new OpenApiInteger(1),
                                        ["projectId"] = new OpenApiInteger(1),
                                        ["assigneeId"] = new OpenApiInteger(123)
                                    }
                                }
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

        return endpoints.MapGet("/tasks/{id}", async (
            [FromRoute] int id,
            ILogger<IApplicationBuilder> logger,
            ITaskRepository repository) =>
        {
            logger.LogInformation("Start getting task with id: {Id}.", id);

            try
            {
                var task = await repository.GetByIdAsync(id);

                if (task is null)
                {
                    logger.LogInformation("Task not found with Id: {Id}.", id);
                    return Results.NotFound();
                }

                logger.LogInformation("Task found with id={Id}.", id);

                return Results.Ok(task);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while getting task with Id: {TaskId}. Operation: {Operation}",
                    id,
                    $"GET /tasks/{id}"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .WithName("GetTaskById")
        .RequireAuthorization()
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Получение задачи по идентификатору."
        })
        .Produces<TaskEntity>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Получение задачи по идентификатору.";
            operation.Description = "Возвращает задачу по указанному идентификатору.";
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

            // Настраиваем ответы
            operation.Responses["200"] = new OpenApiResponse
            {
                Description = "Задача успешно найдена",
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
                            ["GetTask"] = new()
                            {
                                Summary = "Успешное получение задачи",
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

            operation.Responses["404"] = new OpenApiResponse
            {
                Description = "Задача не найдена"
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