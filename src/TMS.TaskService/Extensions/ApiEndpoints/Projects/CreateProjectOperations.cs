using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using TMS.Common.Helpers;
using TMS.Common.Validators;
using TMS.TaskService.Data.Repositories;
using TMS.TaskService.Entities;
using TMS.TaskService.Models.Projects;

namespace TMS.TaskService.Extensions.ApiEndpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class CreateProjectOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// проектов в API:
    ///   POST /projects            → Создание нового проекта;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddCreateProjectOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/projects", async (
                [FromBody] ProjectCreate project,
                [FromServices] ILogger<IApplicationBuilder> logger,
                [FromServices] IMapper mapper,
                [FromServices] IProjectRepository repository) =>
            {
                logger.LogInformation("Start creating project with name: {Name}.", project.Name);

                var result = await ValidateData(project, repository, logger);
                if (!result.IsValid)
                {
                    return ResultHelper.CreateValidationErrorResult(
                    entityName: "Project",
                    entityIdentifier: project.Name,
                    errorMessage: result.ErrorMessage,
                    logger);
                }

                try
                {
                    var entity = mapper.Map<ProjectEntity>(project);

                    await repository.AddAsync(entity);

                    logger.LogInformation("Finish create project with name={ProjectName}.", project.Name);

                    return Results.Created($"api/projects/{entity.Id}", entity);
                }
                catch (Exception ex)
                {
                    return ResultHelper.CreateInternalServerErrorProblemResult($"Error while creating project '{project.Name}'", logger, ex);
                }
            })
            .WithName("CreateProject")
            .RequireAuthorization()
            .WithMetadata(new
            {
                // Для Swagger/документации
                Summary = "Создание нового проекта."
            })
            .Produces<ProjectEntity>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Создание нового проекта.";
                operation.Description = "Создает новый проект с указанными параметрами.";
                OpenApiMigrationHelper.AddTag(operation, "Project");

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
                                    ["name"] = new()
                                    {
                                        Type = "string",
                                        Description = "Название проекта",
                                        MinLength = 1,
                                        MaxLength = 50
                                    },
                                    ["description"] = new()
                                    {
                                        Type = "string",
                                        Description = "Описание проекта",
                                        MaxLength = 500,
                                        Nullable = true
                                    },
                                    ["status"] = new()
                                    {
                                        Type = "integer",
                                        Format = "int32",
                                        Description = "Статус проекта (1-10)",
                                        Minimum = 1,
                                        Maximum = 10,
                                        Enum = new List<IOpenApiAny>
                            {
                                new OpenApiInteger(1),  // Draft
                                new OpenApiInteger(2),  // Pending
                                new OpenApiInteger(3),  // Active
                                new OpenApiInteger(4),  // OnHold
                                new OpenApiInteger(5),  // Completed
                                new OpenApiInteger(6),  // Cancelled
                                new OpenApiInteger(7),  // Archived
                                new OpenApiInteger(8),  // Review
                                new OpenApiInteger(9),  // Planning
                                new OpenApiInteger(10) // Testing
                            }
                                    },
                                    ["userId"] = new()
                                    {
                                        Type = "integer",
                                        Format = "int32",
                                        Description = "Идентификатор пользователя",
                                        Minimum = 1
                                    }
                                },
                                Required = new HashSet<string> { "name", "status", "userId" }
                            },
                            Examples = new Dictionary<string, OpenApiExample>
                            {
                                ["ActiveProject"] = new()
                                {
                                    Summary = "Активный проект",
                                    Value = new OpenApiObject
                                    {
                                        ["name"] = new OpenApiString("Новый проект"),
                                        ["description"] = new OpenApiString("Описание нового проекта"),
                                        ["status"] = new OpenApiInteger(3), // Active
                                        ["userId"] = new OpenApiInteger(123)
                                    }
                                },
                                ["DraftProject"] = new()
                                {
                                    Summary = "Проект в черновике",
                                    Value = new OpenApiObject
                                    {
                                        ["name"] = new OpenApiString("Простой проект"),
                                        ["description"] = new OpenApiString(""),
                                        ["status"] = new OpenApiInteger(1), // Draft
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
                    Description = "Проект успешно создан",
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
                                        MaxLength = 500,
                                        Nullable = true
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
                                    },
                                    ["createdAt"] = new()
                                    {
                                        Type = "string",
                                        Format = "date-time",
                                        Description = "Дата и время создания проекта"
                                    },
                                    ["updatedAt"] = new()
                                    {
                                        Type = "string",
                                        Format = "date-time",
                                        Description = "Дата и время обновления проекта"
                                    },
                                    ["tasks"] = new()
                                    {
                                        Type = "array",
                                        Description = "Список задач проекта",
                                        Items = new OpenApiSchema
                                        {
                                            Type = "object",
                                            Properties = new Dictionary<string, OpenApiSchema>
                                            {
                                                ["id"] = new() { Type = "integer", Format = "int32" },
                                                ["title"] = new() { Type = "string" }
                                            }
                                        },
                                        Nullable = true
                                    }
                                },
                                Required = new HashSet<string> { "id", "name", "status", "userId", "createdAt", "updatedAt" }
                            },
                            Examples = new Dictionary<string, OpenApiExample>
                            {
                                ["ProjectCreated"] = new()
                                {
                                    Summary = "Успешное создание проекта",
                                    Value = new OpenApiObject
                                    {
                                        ["id"] = new OpenApiInteger(1),
                                        ["name"] = new OpenApiString("Новый проект"),
                                        ["description"] = new OpenApiString("Описание нового проекта"),
                                        ["status"] = new OpenApiInteger(3),
                                        ["userId"] = new OpenApiInteger(123),
                                        ["createdAt"] = new OpenApiString("2024-01-15T10:30:00Z"),
                                        ["updatedAt"] = new OpenApiString("2024-01-15T10:30:00Z"),
                                        ["tasks"] = new OpenApiArray()
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
                                        ["detail"] = new OpenApiString("Project Name must be set.")
                                    }
                                },
                                ["DuplicateProject"] = new()
                                {
                                    Summary = "Дублирующийся проект",
                                    Value = new OpenApiObject
                                    {
                                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                                        ["title"] = new OpenApiString("Bad Request"),
                                        ["status"] = new OpenApiInteger(400),
                                        ["detail"] = new OpenApiString("Project with name='Новый проект' already exists.")
                                    }
                                },
                                ["InvalidStatus"] = new()
                                {
                                    Summary = "Некорректный статус",
                                    Value = new OpenApiObject
                                    {
                                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                                        ["title"] = new OpenApiString("Bad Request"),
                                        ["status"] = new OpenApiInteger(400),
                                        ["detail"] = new OpenApiString("Project Status must be set.")
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

    private static async Task<ValidationResult> ValidateData(ProjectCreate project, IProjectRepository repository, ILogger<IApplicationBuilder> logger)
    {
        // Валидация на null
        var projectValidation = CommonValidator.EntityNotNullValidate(project, "Project");
        if (!projectValidation.IsValid)
        {
            return projectValidation;
        }

        var validationResult = ProjectValidator.ProjectValidate(project);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Project validation failed: {ProjectName}, Error: {Error}", project.Name, validationResult.ErrorMessage);
            return validationResult;
        }

        validationResult = ProjectValidator.UserIdValidation(project.UserId);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Project validation failed: {ProjectName}, Error: {Error}", project.Name, validationResult.ErrorMessage);
            return validationResult;
        }

        var trimmedName = project.Name.Trim();
        var projectExists = await repository.IsExistsAsync(trimmedName);
        if (projectExists)
        {
            return ValidationResult.Error($"Project with name='{project.Name}' already exists.");
        }

        return ValidationResult.Success();
    }

    #endregion Private Methods
}