using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;
using TMS.Common.Helpers;
using TMS.Common.RabbitMq;
using TMS.Common.Validators;
using TMS.TaskService.Data.Repositories;

namespace TMS.TaskService.Extensions.ApiEndpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class DeleteTaskOperations
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// задач в API:
    ///   DELETE /tasks/{id}       → Удаление задачи по идентификатору;
    /// </summary>
    /// <param name="endpoints"></param>
    public static RouteHandlerBuilder AddDeleteTaskOperations(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/tasks/{id}", async (
            [FromRoute] int id,
            HttpContext httpContext,
            IRabbitMqService rabbitMqService,
            ILogger<IApplicationBuilder> logger,
            ITaskRepository repository) =>
        {
            logger.LogInformation("Start deleting task with id: {Id}.", id);

            var result = await ValidateData(id, repository, logger);
            if (!result.IsValid)
            {
                return ResultHelper.CreateValidationErrorResult(
                    entityName: "Task",
                    entityIdentifier: id.ToString(),
                    errorMessage: result.ErrorMessage,
                    logger);
            }

            try
            {
                await repository.DeleteAsync(id);

                var message = new TaskMessage(TaskMessageType.Delete, $"Task deleted: ID = {id}.");
                await rabbitMqService.SendMessageAsync(message);

                logger.LogInformation("Finish delete task with id={Id}.", id);

                return Results.NoContent();
            }
            catch (KeyNotFoundException knf)
            {
                logger.LogError(knf, "Task not found with Id: {TaskId}.", id);

                return Results.NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                return ResultHelper.CreateInternalServerErrorProblemResult($"Error while creating task ID={id}", logger, ex);
            }
        })
        .WithName("DeleteTask")
        .RequireAuthorization()
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Удаление задачи по идентификатору."
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Удаление задачи по идентификатору.";
            operation.Description = "Удаляет задачу с указанным идентификатором. Задача должна существовать для успешного удаления.";
            OpenApiMigrationHelper.AddTag(operation, "Task");

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
            operation.Responses["204"] = new OpenApiResponse
            {
                Description = "Задача успешно удалена"
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
                                    ["detail"] = new OpenApiString("Task ID must be valid.")
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
                                Value = new OpenApiString("Task with ID 123 does not exist.")
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
                            },
                            ["DeleteConstraint"] = new()
                            {
                                Summary = "Ошибка ограничения",
                                Value = new OpenApiObject
                                {
                                    ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.6.1"),
                                    ["title"] = new OpenApiString("An error occurred while processing your request."),
                                    ["status"] = new OpenApiInteger(500),
                                    ["detail"] = new OpenApiString("Cannot delete task because it has related comments or attachments")
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
        int taskId,
        ITaskRepository repository,
        ILogger<IApplicationBuilder> logger)
    {
        var taskValidation = await ValidateTaskAsync(taskId, repository, logger);
        if (!taskValidation.IsValid)
        {
            logger.LogWarning("Task validation failed for task {TaskId}: {Error}", taskId, taskValidation.ErrorMessage);
            return taskValidation;
        }

        return ValidationResult.Success();
    }

    private static async Task<ValidationResult> ValidateTaskAsync(int taskId,
                                ITaskRepository repository,
                                ILogger logger)
    {
        try
        {
            var task = await repository.GetByIdAsync(taskId);
            if (task is null)
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