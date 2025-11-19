using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.ComponentModel;
using TMS.AuthService.Data;
using TMS.AuthService.Entities;
using TMS.AuthService.Entities.Enum;
using TMS.AuthService.Models;
using TMS.AuthService.Services;

namespace TMS.AuthService.Extensions.ApiEndpoints;

/// <summary>
///
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Набор методов расширения для IApplicationBuilder, конфигурирующих endpoints
    /// аутентификации в API:
    ///   POST /login      → аутентификация, возврат access/refresh-токенов;
    ///   POST /register   → регистрация пользователя;
    ///   POST /refresh    → обновление токена по refresh-токену.
    /// Включает валидацию, хеширование паролей, генерацию токенов и логирование.
    /// </summary>
    /// <param name="endpoints"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder AddAuthEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/register", async (
                [FromBody] RegisterModel model,
                [FromServices] ILogger<IApplicationBuilder> logger,
                [FromServices] IUserRepository userRepository,
                [FromServices] IHashService hashService) =>
        {
            try
            {
                // Проверка существования пользователя
                if (await userRepository.UserExistsAsync(model.UserName))
                {
                    return Results.BadRequest($"The user with name={model.UserName} already exists.");
                }

                // Хеширование пароля
                var hashedPassword = hashService.HashPassword(model.Password);

                // Создание нового пользователя
                var newUser = new UserEntity
                {
                    UserName = model.UserName,
                    PasswordHash = hashedPassword,
                    Email = model.Email,
                    Role = model.Role
                };

                await userRepository.AddUserAsync(newUser);
                return Results.Created($"/users/{newUser.Id}", new { message = $"The user with name={newUser.UserName} is created." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/register");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
            .WithName("register")
            .Produces<object>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Регистрация пользователя и получение JWT‑токена для входа и обновления.";
                operation.Description = $$"""
                Регистрация пользователя, с указанием логина, пароля, email и роли. И получением JWT токена доступа.

                **Возможные значения для ролей:**
                {{GetUserRolesDescription()}}
                """;

                operation.Tags = new List<OpenApiTag>
                {
                    new OpenApiTag { Name = "Auth" }
                };

                // Для POST запросов используем RequestBody вместо Parameters
                operation.RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Description = "Данные для нового пользователя",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Required = new HashSet<string> { "username", "password" },
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["userName"] = new OpenApiSchema // вместо "username"
                                    {
                                        Type = "string",
                                        Description = "Имя пользователя",
                                        Example = new OpenApiString("admin")
                                    },
                                    ["password"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "password",
                                        Description = "Пароль пользователя",
                                        Example = new OpenApiString("password123")
                                    },
                                    ["email"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description = "Email",
                                        Example = new OpenApiString("admin@mail.box")
                                    },
                                    ["role"] = new OpenApiSchema
                                    {
                                        Type = "integer",
                                        Description = "Роль",
                                        Example = new OpenApiInteger((int)UserRole.User)
                                    }
                                }
                            },
                            Examples = new Dictionary<string, OpenApiExample>
                            {
                                ["RegularUser"] = new OpenApiExample
                                {
                                    Summary = "Регистрация обычного пользователя",
                                    Description = "Пример регистрации обычного пользователя",
                                    Value = new OpenApiObject
                                    {
                                        ["userName"] = new OpenApiString("user1"),
                                        ["password"] = new OpenApiString("user123"),
                                        ["email"] = new OpenApiString("user1@mail.box"),
                                        ["role"] = new OpenApiInteger((int)UserRole.User)
                                    }
                                },
                                ["AdminUser"] = new OpenApiExample
                                {
                                    Summary = "Регистрация администратора",
                                    Description = "Пример регистрации пользователя с правами администратора",
                                    Value = new OpenApiObject
                                    {
                                        ["userName"] = new OpenApiString("admin"),
                                        ["password"] = new OpenApiString("admin123"),
                                        ["email"] = new OpenApiString("admin123@mail.box"),
                                        ["role"] = new OpenApiInteger((int)UserRole.Admin)
                                    }
                                }
                            }
                        }
                    }
                };

                // Удаляем автоматически добавленный 200 Response
                operation.Responses.Remove("200");

                var examples = new Dictionary<string, OpenApiExample>
                {
                    ["UserCreated"] = new OpenApiExample
                    {
                        Summary = "Регистрация",
                        Value = new OpenApiObject
                        {
                            ["message"] = new OpenApiString("The user with name=admin is created.")
                        }
                    }
                };
                EnsureResponseWithExamples(operation, StatusCodes.Status201Created.ToString(), examples);

                // Добавляем другие возможные ответы
                operation.Responses["400"] = new OpenApiResponse
                {
                    Description = "Пользователь с таким именем уже существует."
                };

                operation.Responses["500"] = new OpenApiResponse
                {
                    Description = "Внутренняя ошибка сервера"
                };

                return operation;
            });

        endpoints.MapPost("/login", async (
                [FromBody] LoginModel model,
                [FromServices] ILogger<IApplicationBuilder> logger,
                [FromServices] IUserRepository userRepository,
                [FromServices] ITokenService tokenService,
                [FromServices] IHashService hashService) =>
            {
                logger.LogInformation("Start logging with Username: {Username}.", model.Username);

                try
                {
                    // 1. Поиск пользователя
                    var userExists = await userRepository.UserExistsAsync(model.Username);
                    if (!userExists)
                        return Results.NotFound();

                    var user = (await userRepository.GetByUsernameAsync(model.Username))!;

                    // 2. Проверка пароля
                    if (!hashService.VerifyPassword(user.PasswordHash, model.Password))
                        return Results.NotFound();

                    // 3. Генерация токенов
                    var (accessToken, refreshToken) = await tokenService.GenerateTokensAsync(user);

                    // 4. Возврат результата
                    return Results.Ok(new TokensModel(accessToken, refreshToken));
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Error while logging with Username: {Username}. Operation: {Operation}",
                        model.Username,
                        "POST /login"
                    );

                    return Results.Problem(
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }
            })
            .WithName("login")
            .Produces<TokensModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Аутентификация пользователя и получение JWT‑токена.";
                operation.Description = "Аутентификация пользователя по логину и паролю с получением JWT токена доступа.";

                // Для POST запросов используем RequestBody вместо Parameters
                operation.RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Description = "Данные для аутентификации пользователя",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Required = new HashSet<string> { "username", "password" },
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["username"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description = "Имя пользователя",
                                        Example = new OpenApiString("admin")
                                    },
                                    ["password"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "password",
                                        Description = "Пароль пользователя",
                                        Example = new OpenApiString("password123")
                                    }
                                }
                            },
                            Examples = new Dictionary<string, OpenApiExample>
                            {
                                ["AdminUser"] = new OpenApiExample
                                {
                                    Summary = "Аутентификация администратора",
                                    Description = "Пример запроса для пользователя с правами администратора",
                                    Value = new OpenApiObject
                                    {
                                        ["username"] = new OpenApiString("admin"),
                                        ["password"] = new OpenApiString("admin123")
                                    }
                                },
                                ["RegularUser"] = new OpenApiExample
                                {
                                    Summary = "Аутентификация обычного пользователя",
                                    Description = "Пример запроса для обычного пользователя",
                                    Value = new OpenApiObject
                                    {
                                        ["username"] = new OpenApiString("user1"),
                                        ["password"] = new OpenApiString("user123")
                                    }
                                }
                            }
                        }
                    }
                };

                operation.Tags = new List<OpenApiTag>
                {
                    new OpenApiTag { Name = "Auth" }
                };

                var examples = new Dictionary<string, OpenApiExample>
                {
                    ["AuthSuccess"] = new OpenApiExample
                    {
                        Summary = "Регистрация",
                        Value = new OpenApiObject
                        {
                            ["access_token"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bW..."),
                            ["refresh_token"] = new OpenApiString("mO8zxxdYgC8lJXpRqiN07r0jBYhP/buKOJjaNIKmyfuN+EQPW/BbaOLyAeMmU0...")
                        }
                    }
                };
                EnsureResponseWithExamples(operation, StatusCodes.Status200OK.ToString(), examples);

                // Добавляем другие возможные ответы
                operation.Responses["404"] = new OpenApiResponse
                {
                    Description = "Неверное имя пользователя или пароль."
                };

                operation.Responses["500"] = new OpenApiResponse
                {
                    Description = "Внутренняя ошибка сервера"
                };

                return operation;
            });

        return endpoints.MapPost("/refresh", async (
            [FromBody] TokenRefreshModel model,
            [FromServices] ILogger<IApplicationBuilder> logger,
            [FromServices] ITokenService tokenService) =>
                {
                    try
                    {
                        var (accessToken, refreshToken) = await tokenService.RefreshTokensAsync(model.RefreshToken);
                        return Results.Ok(new TokensModel(accessToken, refreshToken));
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        logger.LogError(ex, "/refresh");
                        return Results.Unauthorized();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "/refresh");
                        return Results.Problem(detail: ex.Message, statusCode: 500);
                    }
                })
        .WithName("refresh")
        .Produces<TokensModel>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Запрос на обновление JWT‑токена.";
            operation.Description = "Обновление access токена с использованием refresh токена.";

            // Для POST запросов используем RequestBody вместо Parameters
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "Refresh токен для обновления access токена",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Required = new HashSet<string> { "refreshToken" },
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["refreshToken"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description = "Refresh токен",
                                    Example = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...")
                                }
                            }
                        },
                        Examples = new Dictionary<string, OpenApiExample>
                        {
                            ["RefreshExample"] = new OpenApiExample
                            {
                                Summary = "Пример запроса на обновление токена",
                                Description = "Типичный запрос для обновления access токена",
                                Value = new OpenApiObject
                                {
                                    ["refreshToken"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...")
                                }
                            }
                        }
                    }
                }
            };

            operation.Tags = new List<OpenApiTag>
            {
                new OpenApiTag { Name = "Auth" }
            };

            var examples = new Dictionary<string, OpenApiExample>
            {
                ["TokenRefreshed"] = new OpenApiExample
                {
                    Summary = "Обновление",
                    Value = new OpenApiObject
                    {
                        ["access_token"] = new OpenApiString("y8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJodHRwOi8v..."),
                        ["refresh_token"] = new OpenApiString("mO8zxxdYgC8lJXpRqiN07r0jBYhP/buKOJjaNIKmyfuN+EQPW/BbaOLyAeMmU0...")
                    }
                }
            };
            EnsureResponseWithExamples(operation, StatusCodes.Status200OK.ToString(), examples);

            // Добавляем другие возможные ответы
            operation.Responses["401"] = new OpenApiResponse
            {
                Description = "Неверный или просроченный refresh токен"
            };

            operation.Responses["500"] = new OpenApiResponse
            {
                Description = "Внутренняя ошибка сервера"
            };

            return operation;
        });
    }

    #region Private Methods

    private static string GetUserRolesDescription()
    {
        var roles = Enum.GetValues(typeof(UserRole))
            .Cast<UserRole>()
            .Select(role => $"- {(int)role}: {role} - {GetEnumDescription(role)}")
            .ToArray();

        return string.Join("\n", roles);
    }

    private static string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        return attribute?.Description ?? value.ToString();
    }

    private static void EnsureResponseWithExamples(
        OpenApiOperation operation,
        string statusCode,
        Dictionary<string, OpenApiExample> examples)
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

    #endregion Private Methods
}