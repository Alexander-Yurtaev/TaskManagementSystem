using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace TMS.Common.Helpers;

public static class OpenApiMigrationHelper
{
    public static OpenApiOperation InitOperationForSetup(OpenApiOperation operation, string dbName, string tag)
    {
        operation = BaseInitOperationForMigration(operation, dbName);

        AddTag(operation, tag);

        operation.Description = """
                ### Условия доступа
                * **Без авторизации**

                ### Описание
                Эндпоинт для выполнения настройки базы данных. Если БД уже создана, то выполнение будет прервано.
                """;

        // Добавляем другие возможные ответы
        operation.Responses["400"] = new OpenApiResponse
        {
            Description = "Система уже настроена."
        };

        return operation;
    }

    public static OpenApiOperation InitOperationForMigration(OpenApiOperation operation, string dbName, string tag)
    {
        operation = BaseInitOperationForMigration(operation, dbName);

        AddTag(operation, tag);

        operation.Description = """
                ### Условия доступа
                * **Требуется роль Admin** — если БД существует (есть хотя бы одна миграция)

                ### Описание
                Эндпоинт для выполнения миграций базы данных. Требуется наличие роли Admin для выполнения миграций.
                """;

        // Добавляем другие возможные ответы
        //operation.Responses["401"] = new OpenApiResponse
        //{
        //    Description = "Неверный или просроченный refresh токен"
        //};

        return operation;
    }

    public static void EnsureResponseWithExamples(OpenApiOperation operation, string statusCode, Dictionary<string, OpenApiExample> examples)
    
    {
        if (!operation.Responses.TryGetValue(statusCode, out var response))
        {
            response = new OpenApiResponse
            {
                Description = GetDefaultDescription(statusCode),
                Content = new Dictionary<string, OpenApiMediaType>()
            };
            operation.Responses.Add(statusCode, response);
        }

        response.Content ??= new Dictionary<string, OpenApiMediaType>();

        if (!response.Content.ContainsKey("application/json"))
        {
            response.Content["application/json"] = new OpenApiMediaType();
        }

        response.Content["application/json"].Examples = examples;
    }

    #region Private Methods

    private static OpenApiOperation BaseInitOperationForMigration(OpenApiOperation operation, string dbName)
    {
        var openApiExampleMessage = $"База данных {dbName} успешно обновлена!";

        operation.Summary = "Запуск миграции БД для Сервиса аутентификации.";

        var examples = new Dictionary<string, OpenApiExample>()
        {
            ["FirstMigration"] = new OpenApiExample
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
            },
            ["NoPendingMigrations"] = new OpenApiExample
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
            },
            ["NextMigrations"] = new OpenApiExample
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
            }
        };

        EnsureResponseWithExamples(operation, StatusCodes.Status200OK.ToString(), examples);

        operation.Responses["500"] = new OpenApiResponse
        {
            Description = "Внутренняя ошибка сервера"
        };

        return operation;
    }

    private static string GetDefaultDescription(string statusCode) => statusCode switch
    {
        "200" => "Успешный запрос",
        "201" => "Успешное создание",
        "400" => "Неверный запрос",
        "401" => "Неавторизован",
        "404" => "Не найдено",
        "500" => "Внутренняя ошибка сервера",
        _ => "Ответ"
    };

    public static void AddTag(OpenApiOperation operation, string tag)
    {
        operation.Tags = new List<OpenApiTag>
        {
            new OpenApiTag { Name = tag }
        };
    }

    #endregion Private Methods
}
