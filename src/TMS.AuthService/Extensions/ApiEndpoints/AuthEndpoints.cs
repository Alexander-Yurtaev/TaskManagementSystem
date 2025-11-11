using Microsoft.AspNetCore.Mvc;
using TMS.AuthService.Data;
using TMS.AuthService.Entities;
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
            .WithMetadata(new
            {
                // Для Swagger/документации
                Summary = "Аутентификация пользователя и получение JWT‑токена."
            });

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
            .WithMetadata(new
            {
                // Для Swagger/документации
                Summary = "Регистрация пользователя."
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
        .WithMetadata(new
        {
            // Для Swagger/документации
            Summary = "Запрос на обновление JWT‑токена."
        });
    }
}