using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using TMS.Common.Helpers;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;

namespace TMS.TaskService.Extensions.ApiEndpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class ReadProjectOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// проектов в API:
    ///   GET /projects            → Получение списка всех проектов;
    ///   GET /projects/{id}       → Получение проекта по идентификатору;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddReadProjectOperations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/projects", async (
            HttpContext httpContext,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IProjectRepository repository) =>
        {
            logger.LogInformation("Start get all projects.");

            // Получение ID пользователя из claims
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                logger.LogWarning("User ID not found in claims");
                return Results.Unauthorized();
            }

            try
            {
                var projects = (await repository.GetProjectsByUserIdAsync(currentUserId)).ToArray();

                logger.LogInformation("Found {ProjectsCount} projects.", projects.Length);

                return Results.Ok(projects);
            }
            catch (Exception ex)
            {
                return ResultHelper.CreateInternalServerErrorProblemResult(logger, ex);
            }
        })
        .WithName("GetProjects")
        .RequireAuthorization()
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Получение списка всех проектов."
        })
        .Produces<ProjectEntity[]?>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Получение всех проектов, прикрепленных к текущему пользователю.";
            operation.Description = "Возвращает список всех проектов, связанных с текущим пользователем.";
            OpenApiHelper.AddTag(operation, "Project");

            // Настраиваем ответы
            operation.Responses["200"] = new OpenApiResponse
            {
                Description = "Список проектов успешно получен",
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
                                        Description = "Идентификатор проекта"
                                    },
                                    ["name"] = new()
                                    {
                                        Type = "string",
                                        Description = "Название проекта",
                                        MaxLength = 50
                                    },
                                    ["description"] = new()
                                    {
                                        Type = "string",
                                        Description = "Описание проекта",
                                        MaxLength = 500
                                    },
                                    ["createdAt"] = new()
                                    {
                                        Type = "string",
                                        Format = "date-time",
                                        Description = "Дата создания проекта"
                                    },
                                    ["updatedAt"] = new()
                                    {
                                        Type = "string",
                                        Format = "date-time",
                                        Description = "Дата последнего обновления проекта"
                                    },
                                    ["status"] = new()
                                    {
                                        Type = "integer",
                                        Format = "int32",
                                        Description = "Статус проекта"
                                    },
                                    ["userId"] = new()
                                    {
                                        Type = "integer",
                                        Format = "int32",
                                        Description = "Идентификатор пользователя"
                                    }
                                },
                                Required = new HashSet<string> { "id", "name", "createdAt", "updatedAt", "status", "userId" }
                            }
                        },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["GetProjects"] = new()
                            {
                                Summary = "Успешное получение списка проектов",
                                Value = new OpenApiArray
                                {
                                    new OpenApiObject
                                    {
                                        ["id"] = new OpenApiInteger(1),
                                        ["name"] = new OpenApiString("Проект 1"),
                                        ["description"] = new OpenApiString("Описание проекта 1"),
                                        ["createdAt"] = new OpenApiString("2024-01-01T10:00:00Z"),
                                        ["updatedAt"] = new OpenApiString("2024-01-02T10:00:00Z"),
                                        ["status"] = new OpenApiInteger(1),
                                        ["userId"] = new OpenApiInteger(123)
                                    },
                                    new OpenApiObject
                                    {
                                        ["id"] = new OpenApiInteger(2),
                                        ["name"] = new OpenApiString("Проект 2"),
                                        ["description"] = new OpenApiString("Описание проекта 2"),
                                        ["createdAt"] = new OpenApiString("2024-01-03T10:00:00Z"),
                                        ["updatedAt"] = new OpenApiString("2024-01-04T10:00:00Z"),
                                        ["status"] = new OpenApiInteger(2),
                                        ["userId"] = new OpenApiInteger(123)
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

        return endpoints.MapGet("/projects/{id}", async (
            [FromRoute] int id,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IProjectRepository repository) =>
        {
            logger.LogInformation("Start getting project with id: {Id}.", id);

            try
            {
                var project = await repository.GetByIdAsync(id);

                if (project is null)
                {
                    logger.LogInformation("Project not found with Id: {Id}.", id);

                    return Results.NotFound();
                }

                logger.LogInformation("Project found with id={Id}.", id);

                return Results.Ok(project);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error while getting project with Id: {ProjectId}. Operation: {Operation}",
                    id,
                    $"GET /projects/{id}"
                );

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .WithName("GetProjectById")
        .RequireAuthorization()
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Получение проекта по идентификатору."
        })
        .Produces<ProjectEntity>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Получение проекта по идентификатору.";
            operation.Description = "Возвращает проект по указанному идентификатору.";
            OpenApiHelper.AddTag(operation, "Project");

            // Добавляем параметры
            operation.Parameters = new List<OpenApiParameter>
            {
                new()
                {
                    Name = "id",
                    In = ParameterLocation.Path,
                    Required = true,
                    Description = "Идентификатор проекта",
                    Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                }
            };

            // Настраиваем ответы
            operation.Responses["200"] = new OpenApiResponse
            {
                Description = "Проект успешно найден",
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
                                    Description = "Идентификатор проекта"
                                },
                                ["name"] = new()
                                {
                                    Type = "string",
                                    Description = "Название проекта",
                                    MaxLength = 50
                                },
                                ["description"] = new()
                                {
                                    Type = "string",
                                    Description = "Описание проекта",
                                    MaxLength = 500
                                },
                                ["createdAt"] = new()
                                {
                                    Type = "string",
                                    Format = "date-time",
                                    Description = "Дата создания проекта"
                                },
                                ["updatedAt"] = new()
                                {
                                    Type = "string",
                                    Format = "date-time",
                                    Description = "Дата последнего обновления проекта"
                                },
                                ["status"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Статус проекта"
                                },
                                ["userId"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Идентификатор пользователя"
                                }
                            },
                            Required = new HashSet<string> { "id", "name", "createdAt", "updatedAt", "status", "userId" }
                        },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["GetProject"] = new()
                            {
                                Summary = "Успешное получение проекта",
                                Value = new OpenApiObject
                                {
                                    ["id"] = new OpenApiInteger(1),
                                    ["name"] = new OpenApiString("Проект 1"),
                                    ["description"] = new OpenApiString("Описание проекта 1"),
                                    ["createdAt"] = new OpenApiString("2024-01-01T10:00:00Z"),
                                    ["updatedAt"] = new OpenApiString("2024-01-02T10:00:00Z"),
                                    ["status"] = new OpenApiInteger(1),
                                    ["userId"] = new OpenApiInteger(123)
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses["404"] = new OpenApiResponse
            {
                Description = "Проект не найден"
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