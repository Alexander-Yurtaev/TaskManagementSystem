using TMS.Entities.Auth;

namespace TMS.AuthService.Services;

/// <summary>
/// 
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<(string, string)> GenerateTokensAsync(UserEntity user);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="storedHash"></param>
    /// <param name="providedToken"></param>
    /// <returns></returns>
    bool ValidateRefreshToken(string storedHash, string providedToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    Task<(string, string)> RefreshTokensAsync(string refreshToken);
}