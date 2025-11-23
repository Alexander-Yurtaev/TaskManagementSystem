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
public static class UpdateProjectOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// проектов в API:
    ///   PUT /projects/{id}       → Обновление проекта по идентификатору;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddUpdateProjectOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/projects/{id}", async (
            [FromRoute] int id,
            [FromBody] ProjectUpdate projectUpdate,
            HttpContext httpContext,
            ILogger<IApplicationBuilder> logger,
            IMapper mapper,
            IProjectRepository repository) =>
        {
            logger.LogInformation("Start updating project with id: {Id}.", id);

            // Валидация данных
            var validationResult = await ValidateData(id, projectUpdate, repository, logger);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Project validation failed for project {ProjectId}: {Error}", id, validationResult.ErrorMessage);
                return Results.BadRequest(validationResult.ErrorMessage);
            }

            try
            {
                var project = await repository.GetByIdAsync(id);

                if (project is null)
                {
                    logger.LogWarning("Project not found with id: {Id}.", id);
                    return Results.NotFound($"Project with id={id} does not exist.");
                }

                // Обновление проекта
                mapper.Map(projectUpdate, project);
                project.UpdatedAt = DateTime.UtcNow;

                project = await repository.UpdateAsync(project);

                logger.LogInformation("Project updated successfully with id: {Id}.", id);
                return Results.Ok(project);
            }
            catch (Exception ex)
            {
                return ResultHelper.CreateInternalServerErrorProblemResult(logger, ex);
            }
        })
        .WithName("UpdateProject")
        .RequireAuthorization()
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Обновление проекта по идентификатору."
        })
        .Produces<ProjectEntity>(StatusCodes.Status200OK)
        .Produces<string>(StatusCodes.Status400BadRequest)
        .Produces<string>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Обновление проекта по идентификатору.";
            operation.Description = "Обновляет проект с указанным идентификатором.";
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
                                ["status"] = new()
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Статус проекта"
                                }
                            },
                            Required = new HashSet<string> { "name" }
                        },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["UpdateProject"] = new()
                            {
                                Summary = "Пример обновления проекта",
                                Value = new OpenApiObject
                                {
                                    ["name"] = new OpenApiString("Обновленное название проекта"),
                                    ["description"] = new OpenApiString("Обновленное описание проекта"),
                                    ["status"] = new OpenApiInteger(2)
                                }
                            }
                        }
                    }
                }
            };

            // Настраиваем ответы
            operation.Responses["200"] = new OpenApiResponse
            {
                Description = "Проект успешно обновлен",
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
                            ["UpdatedProject"] = new()
                            {
                                Summary = "Успешно обновленный проект",
                                Value = new OpenApiObject
                                {
                                    ["id"] = new OpenApiInteger(1),
                                    ["name"] = new OpenApiString("Обновленное название проекта"),
                                    ["description"] = new OpenApiString("Обновленное описание проекта"),
                                    ["createdAt"] = new OpenApiString("2024-01-01T10:00:00Z"),
                                    ["updatedAt"] = new OpenApiString("2024-01-15T10:00:00Z"),
                                    ["status"] = new OpenApiInteger(2),
                                    ["userId"] = new OpenApiInteger(123)
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses["400"] = new OpenApiResponse
            {
                Description = "Неверные данные запроса",
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
                                Value = new OpenApiString("Project name is required")
                            }
                        }
                    }
                }
            };

            operation.Responses["404"] = new OpenApiResponse
            {
                Description = "Проект не найден",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new()
                    {
                        Schema = new OpenApiSchema { Type = "string" },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["NotFound"] = new()
                            {
                                Summary = "Проект не найден",
                                Value = new OpenApiString("Project with id=1 does not exist.")
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
            int projectId,
            ProjectUpdate projectUpdate,
            IProjectRepository repository,
            ILogger logger)
    {
        // Валидация существования проекта
        var projectValidation = await ValidateProjectAsync(projectId, repository, logger);
        if (!projectValidation.IsValid)
        {
            return projectValidation;
        }

        // Валидация на null
        projectValidation = CommonValidator.EntityNotNullValidate(projectUpdate, "Project");
        if (!projectValidation.IsValid)
        {
            return projectValidation;
        }

        // Валидация данных проекта
        var validationResult = ProjectValidator.ProjectValidate(projectUpdate);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        logger.LogInformation("All validations passed for project {ProjectId}", projectId);
        return ValidationResult.Success();
    }

    private static async Task<ValidationResult> ValidateProjectAsync(
        int projectId,
        IProjectRepository projectRepository,
        ILogger logger)
    {
        try
        {
            var isExists = await projectRepository.IsExistsAsync(projectId);
            if (!isExists)
            {
                return ValidationResult.Error($"Project with ID {projectId} does not exist.");
            }
            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating project existence for project {ProjectId}", projectId);
            return ValidationResult.Error("Unable to validate project existence.");
        }
    }

    #endregion Private Methods
}