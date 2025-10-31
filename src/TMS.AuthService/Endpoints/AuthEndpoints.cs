using Microsoft.AspNetCore.Mvc;
using TMS.AuthService.Data;
using TMS.AuthService.Models;
using TMS.AuthService.Services;
using TMS.Entities.Auth;

namespace TMS.AuthService.Endpoints;

/// <summary>
/// 
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder AddAuthEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("api/login", async (
                [FromBody] LoginModel model,
                [FromServices] IUserRepository userRepository,
                [FromServices] ITokenService tokenService,
                [FromServices] IHashService hashService) =>
            {
                try
                {
                    // 1. Поиск пользователя
                    var user = await userRepository.GetByUsernameAsync(model.Username);
                    if (user is null)
                        return Results.Unauthorized();

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
                    return Results.Problem(
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }
            })
            .WithName("login")
            .AllowAnonymous();

        endpoints.MapPost("api/register", async (
                [FromBody] RegisterModel model,
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
                    return Results.Created("/api/users", new { message = "Пользователь создан" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }
            })
            .WithName("register")
            .AllowAnonymous();

        return endpoints.MapPost("/api/refresh", async (
            [FromBody] TokenRefreshModel model,
            [FromServices] ITokenService tokenService) =>
        {
            try
            {
                var (accessToken, refreshToken) = await tokenService.RefreshTokensAsync(model.RefreshToken);
                return Results.Ok(new TokensModel(accessToken, refreshToken));
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        })
        .WithName("refresh")
        .AllowAnonymous();
    }
}

