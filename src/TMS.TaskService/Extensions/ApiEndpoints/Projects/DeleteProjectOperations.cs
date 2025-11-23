using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;
using TMS.Common.Helpers;
using TMS.Common.Validators;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.ApiEndpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class DeleteProjectOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// проектов в API:
    ///   DELETE /projects/{id}            → Удаление проекта по идентификатору;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddDeleteProjectOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/projects/{id}", async (
            [FromRoute] int id,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] IProjectRepository repository) =>
        {
            logger.LogInformation("Start deleting project with id: {Id}.", id);

            var result = await ValidateData(id, repository, logger);
            if (!result.IsValid)
            {
                return ResultHelper.CreateValidationErrorResult(
                    entityName: "Project",
                    entityIdentifier: id.ToString(),
                    errorMessage: result.ErrorMessage,
                    logger);
            }

            try
            {
                await repository.DeleteAsync(id);

                logger.LogInformation("Finish delete project with id={Id}.", id);

                return Results.NoContent();
            }
            catch (KeyNotFoundException knf)
            {
                return ResultHelper.CreateProblemResult(
                                detail: "Project not found",
                                statusCode: StatusCodes.Status404NotFound,
                                logger,
                                knf);
            }
            catch (Exception ex)
            {
                return ResultHelper.CreateInternalServerErrorProblemResult($"Error while deleting project ID={id}", logger, ex);
            }
        })
        .WithName("DeleteProject")
        .RequireAuthorization()
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Удаление проекта по идентификатору."
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Удаление проекта по идентификатору.";
            operation.Description = "Удаляет проект с указанным идентификатором. Проект должен существовать и не иметь связанных задач для успешного удаления.";
            OpenApiMigrationHelper.AddTag(operation, "Project");

            // Добавляем параметры
            operation.Parameters = new List<OpenApiParameter>
            {
                new()
                {
                    Name = "id",
                    In = ParameterLocation.Path,
                    Required = true,
                    Description = "Идентификатор проекта",
                    Schema = new OpenApiSchema { Type = "integer", Format = "int32", Minimum = 1 }
                }
            };

            // Настраиваем ответы
            operation.Responses["204"] = new OpenApiResponse
            {
                Description = "Проект успешно удален"
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
                                    ["detail"] = new OpenApiString("Project with ID 123 does not exist.")
                                }
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
                            ["NotFound"] = new()
                            {
                                Summary = "Проект не найден",
                                Value = new OpenApiObject
                                {
                                    ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.4"),
                                    ["title"] = new OpenApiString("Not Found"),
                                    ["status"] = new OpenApiInteger(404),
                                    ["detail"] = new OpenApiString("Project not found")
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

    private static async Task<ValidationResult> ValidateData(
        int projectId,
        IProjectRepository repository,
        ILogger<IApplicationBuilder> logger)
    {
        var projectValidation = await ValidateProjectAsync(projectId, repository);
        if (!projectValidation.IsValid)
        {
            logger.LogWarning("Project validation failed for project {ProjectId}: {Error}", projectId, projectValidation.ErrorMessage);
            return projectValidation;
        }

        return ValidationResult.Success();
    }

    private static async Task<ValidationResult> ValidateProjectAsync(int projectId,
                                IProjectRepository repository,
                                ILogger? logger = null)
    {
        try
        {
            var project = await repository.GetByIdAsync(projectId);
            if (project is null)
            {
                return ValidationResult.Error($"Project with ID {projectId} does not exist.");
            }

            // Проверка на наличие связанных задач
            if (project.Tasks?.Any() == true)
            {
                return ValidationResult.Error($"Project with ID {projectId} has associated tasks and cannot be deleted.");
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error validating project existence for project {ProjectId}", projectId);
            return ValidationResult.Error("Unable to validate project existence.");
        }
    }

    #endregion Private Methods
}