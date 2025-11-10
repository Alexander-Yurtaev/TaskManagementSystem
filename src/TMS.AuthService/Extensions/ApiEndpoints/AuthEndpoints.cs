using Microsoft.AspNetCore.Mvc;
using TMS.AuthService.Data;
using TMS.AuthService.Entities;
using TMS.AuthService.Models;
using TMS.AuthService.Services;

namespace TMS.AuthService.Extensions.Endpoints;

/// <summary>
///
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder AddAuthEndpoint(this IApplicationBuilder app)
    {
        var endpoints = (IEndpointRouteBuilder)app;

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
                        return Results.Unauthorized();

                    var user = (await userRepository.GetByUsernameAsync(model.Username))!;

                    // 2. Проверка пароля
                    if (!hashService.VerifyPassword(user.PasswordHash, model.Password))
                        return Results.Unauthorized();

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
            .AllowAnonymous();

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
                        return Results.BadRequest("Пользователь уже существует");

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
                    return Results.Created($"/users/{newUser.Id}", new { message = "Пользователь создан" });
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "/register");
                    return Results.Problem(
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }
            })
            .WithName("register")
            .AllowAnonymous();

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
                logger?.LogError(ex, "/refresh");
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "/refresh");
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        })
        .WithName("refresh")
        .AllowAnonymous();
    }
}