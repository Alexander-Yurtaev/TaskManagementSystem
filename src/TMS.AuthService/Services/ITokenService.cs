using TMS.AuthService.Entities;

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
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    Task<(string, string)> RefreshTokensAsync(string refreshToken);
}