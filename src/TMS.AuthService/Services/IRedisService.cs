namespace TMS.AuthService.Services;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRedisService<T>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    Task SetAsync(string key, T value, TimeSpan expiry);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<T?> GetAsync(string key);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    Task UpdateExpiryAsync(string key, TimeSpan expiry);
}