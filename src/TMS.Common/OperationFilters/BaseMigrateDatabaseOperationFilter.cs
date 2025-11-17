using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TMS.Common.OperationFilters;

/// <summary>
/// 
/// </summary>
public abstract class BaseMigrateDatabaseOperationFilter : IOperationFilter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.OperationId != "MigrateDatabase")
            return;

        operation.Summary = "Запуск миграции БД";
        operation.Description = "Выполняет миграции и возвращает статус: список применённых и ожидающих миграций.";

        // Настройка примеров ответов
        var okResponse = operation.Responses["200"];
        okResponse.Description = "Миграция выполнена. Возвращает информацию о применённых и ожидающих миграциях.";

        var content = okResponse.Content.First(c => c.Key.StartsWith("application/json")).Value;
        var openApiExampleMessage = $"База данных {DatabaseName} успешно обновлена!";

        // Пример 1: «Самая первая миграция»
        content.Examples.Add(
            "FirstMigration", new OpenApiExample
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
            });

        // Пример 2: «Нет текущих миграций» (pendingMigrations пустой)
        content.Examples.Add(
            "NoPendingMigrations", new OpenApiExample
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
            });

        // Пример 3: «Последующие миграции»
        content.Examples.Add(
            "NextMigrations", new OpenApiExample
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
            });
    }

    public abstract string DatabaseName { get; }
}
