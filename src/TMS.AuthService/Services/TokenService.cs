using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TMS.AuthService.Data;
using TMS.AuthService.Models;
using TMS.Entities.Auth;

namespace TMS.AuthService.Services;

/// <summary>
/// 
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IRedisService<UserToken> _redisService;
    private readonly IUserRepository _repository;
    private readonly IHashService _hashService;
    private readonly ILogger<TokenService> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="redisService"></param>
    /// <param name="repository"></param>
    /// <param name="hashService"></param>
    /// <param name="logger"></param>
    public TokenService(IConfiguration configuration, 
        IRedisService<UserToken> redisService, 
        IUserRepository repository,
        IHashService hashService,
        ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _redisService = redisService;
        _repository = repository;
        _hashService = hashService;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<(string, string)> GenerateTokensAsync(UserEntity user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user);
        return (accessToken, refreshToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="storedHash"></param>
    /// <param name="providedToken"></param>
    /// <returns></returns>
    public bool ValidateRefreshToken(string storedHash, string providedToken)
    {
        return BCrypt.Net.BCrypt.Verify(providedToken, storedHash);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<(string, string)> RefreshTokensAsync(string refreshToken)
    {
        // Получаем пользователя по refresh token
        var user = await GetUserByRefreshToken(refreshToken);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Генерируем новый access token
        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = await GenerateRefreshTokenAsync(user);

        // Обновляем время жизни refresh token в Redis
        var expiry = TimeSpan.FromDays(Convert.ToDouble(_configuration["RefreshTokenLifetime"]));
        await _redisService.UpdateExpiryAsync(refreshToken, expiry);

        return (newAccessToken, newRefreshToken);
    }

    private async Task<UserEntity?> GetUserByRefreshToken(string refreshToken)
    {
        // Хеш токена используется как ключ
        var hashedRefreshToken = HashToken(refreshToken);
        var userToken = await _redisService.GetAsync(hashedRefreshToken);
        if (userToken is null)
        {
            return null;
        }

        var user = await _repository.GetByIdAsync(userToken.UserId);
        return user;
    }

    #region Private Methods

    private string GenerateAccessToken(UserEntity user)
    {
        var jwtKey = _configuration["JWT_KEY"];
        var jwtIssue = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssue) || string.IsNullOrEmpty(jwtAudience))
        {
            throw new Exception("JWT configuration is not properly set up");
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashToken(string token)
    {
        var hashedToken = _hashService.HashToken(token);
        return hashedToken;
    }

    private async Task<string> GenerateRefreshTokenAsync(UserEntity user)
    {
        // Генерируем случайный токен
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        var refreshToken = Convert.ToBase64String(randomNumber);

        // Хешируем токен для безопасного хранения
        var hashedRefreshToken = HashToken(refreshToken);
        _logger.LogInformation("TokenService: GenerateRefreshTokenAsync");
        _logger.LogInformation($"refresh: {refreshToken}; hash: {hashedRefreshToken}");

        // Сохраняем токен в базе данных (если необходимо)
        await SaveRefreshTokenAsync(hashedRefreshToken, user.Id);

        return refreshToken;
    }

    private async Task SaveRefreshTokenAsync(string hashedRefreshToken, int userId)
    {
        var expireDays = Convert.ToDouble(_configuration["RefreshTokenLifetime"]);
        var expiry = TimeSpan.FromDays(expireDays);
        var expiryDateTime = DateTime.UtcNow.AddDays(expireDays);
        var userToken = new UserToken(userId, hashedRefreshToken, expiryDateTime);
        await _redisService.SetAsync(hashedRefreshToken, userToken, expiry);
    }

    #endregion Private Methods
}