using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TMS.AuthService.Extensions.ApiEndpoints.OperationFilters;

/// <summary>
/// 
/// </summary>
public class MigrationSecurityFilter : IOperationFilter
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

        // Добавляем описание условий доступа
        operation.Description = """
        ### Условия доступа

        * **Без авторизации** — если БД не создана (нет ни одной миграции)
        * **Требуется роль Admin** — если БД существует (есть хотя бы одна миграция)

        ### Описание
        Эндпоинт для выполнения миграций базы данных. При первом запуске миграции могут быть выполнены без авторизации. 
        После создания БД требуется наличие роли Admin для выполнения дальнейших миграций.
        """;

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
    }
}