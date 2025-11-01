using System.Security.Cryptography;
using System.Text;

namespace TMS.AuthService.Services;

/// <summary>
/// 
/// </summary>
public class HashService : IHashService
{
    private const int Workfactor = 12; // Рекомендуемое значение для BCrypt
    private readonly SHA256 _sha256 = SHA256.Create();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public string HashPassword(string password)
    {
        // Генерируем соль и хешируем пароль
        var bcryptHash = BCrypt.Net.BCrypt.HashPassword(password, Workfactor);
        return bcryptHash;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public bool VerifyPassword(string hash, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public string HashToken(string token)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hashBytes = _sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}