using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace TMS.Common.Helpers;

public static class OpenApiHelper
{
    public static OpenApiOperation InitOperationForInitialMigration(OpenApiOperation operation, string dbName)
    {
        operation = InitOperationForMigration(operation, dbName);

        operation.Tags = new List<OpenApiTag>
        {
            new OpenApiTag { Name = "Auth" }
        };

        operation.Description = """
                ### Условия доступа
                * **Без авторизации** — если БД не создана (нет ни одной миграции)
                * **Требуется роль Admin** — если БД существует (есть хотя бы одна миграция)

                ### Описание
                Эндпоинт для выполнения миграций базы данных. При первом запуске миграции могут быть выполнены без авторизации. 
                После создания БД требуется наличие роли Admin для выполнения дальнейших миграций.
                """;

        return operation;
    }

    public static OpenApiOperation InitOperationForMigration(OpenApiOperation operation, string dbName)
    {
        operation = BaseInitOperationForMigration(operation, dbName);

        operation.Description = """
                ### Условия доступа
                * **Без авторизации** — если БД не создана (нет ни одной миграции)
                * **Требуется роль Admin** — если БД существует (есть хотя бы одна миграция)

                ### Описание
                Эндпоинт для выполнения миграций базы данных. При первом запуске миграции могут быть выполнены без авторизации. 
                После создания БД требуется наличие роли Admin для выполнения дальнейших миграций.
                """;

        return operation;
    }

    #region Private Methods

    private static OpenApiOperation BaseInitOperationForMigration(OpenApiOperation operation, string dbName)
    {
        // Настраиваем схему безопасности
        operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "bearer",
                                In = ParameterLocation.Header
                            },
                            new List<string> { "Admin" }
                        }
                    }
                };

        var openApiExampleMessage = $"База данных {dbName} успешно обновлена!";

        operation.Summary = "Запуск миграции БД для Сервиса аутентификации.";

        if (!operation.Responses.TryGetValue("200", out var response200))
        {
            response200 = new OpenApiResponse
            {
                Description = "Успешное выполнение миграции",
                Content = new Dictionary<string, OpenApiMediaType>()
            };
            operation.Responses.Add("200", response200);
        }

        // Убедимся, что Content существует
        response200.Content ??= new Dictionary<string, OpenApiMediaType>();

        if (!response200.Content.ContainsKey("application/json"))
        {
            response200.Content["application/json"] = new OpenApiMediaType();
        }

        #region Examples

        if (response200.Content["application/json"].Examples is null)
        {
            response200.Content["application/json"].Examples = new Dictionary<string, OpenApiExample>();
        }
        var examples = response200.Content["application/json"].Examples;

        examples["FirstMigration"] = new OpenApiExample
        {
            Summary = "Самая первая миграция",
            Value = new OpenApiObject
            {
                ["message"] = new OpenApiString(openApiExampleMessage),
                ["pendingMigrations"] = new OpenApiArray
                                            {
                                                new OpenApiString("20251102005013_InitialMigrations")
                                            },
                ["appliedMigrations"] = new OpenApiArray
                                            {
                                                new OpenApiString("20251102005013_InitialMigrations")
                                            }
            }
        };

        examples["NoPendingMigrations"] = new OpenApiExample
        {
            Summary = "Нет текущих миграций",
            Value = new OpenApiObject
            {
                ["message"] = new OpenApiString(openApiExampleMessage),
                ["pendingMigrations"] = new OpenApiArray(),
                ["appliedMigrations"] = new OpenApiArray
                                            {
                                                new OpenApiString("20251102005013_InitialMigrations"),
                                                new OpenApiString("20251102005014_AddTaskServiceMigrations")
                                            }
            }
        };

        examples["NextMigrations"] = new OpenApiExample
        {
            Summary = "Последующие миграции",
            Value = new OpenApiObject
            {
                ["message"] = new OpenApiString(openApiExampleMessage),
                ["pendingMigrations"] = new OpenApiArray
                                            {
                                                new OpenApiString("20251102005014_ChangeTypeOfSomeFieldsServiceMigrations")
                                            },
                ["appliedMigrations"] = new OpenApiArray
                                            {
                                                new OpenApiString("20251102005013_InitialMigrations"),
                                                new OpenApiString("20251102005014_AddTaskServiceMigrations"),
                                                new OpenApiString("20251102005014_ChangeTypeOfSomeFieldsServiceMigrations")
                                            }
            }
        };

        #endregion Examples

        return operation;
    }

    #endregion Private Methods
}
