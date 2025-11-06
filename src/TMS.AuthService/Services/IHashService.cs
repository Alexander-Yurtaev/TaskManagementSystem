namespace TMS.AuthService.Services;

/// <summary>
/// 
/// </summary>
public interface IHashService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    string HashPassword(string password);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    bool VerifyPassword(string hash, string password);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    string HashToken(string token);
}