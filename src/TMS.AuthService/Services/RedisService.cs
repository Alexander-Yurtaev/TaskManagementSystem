using StackExchange.Redis;
using System.Text.Json;

namespace TMS.AuthService.Services;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class RedisService<T> : IRedisService<T> where T : class
{
    private readonly ILogger<RedisService<T>> _logger;
    private readonly IDatabase _database;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="redis"></param>
    /// <param name="logger"></param>
    public RedisService(ConnectionMultiplexer redis, ILogger<RedisService<T>> logger)
    {
        _logger = logger;
        _database = redis.GetDatabase();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiry"></param>
    public async Task SetAsync(string key, T value, TimeSpan expiry)
    {
        var serialized = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(
            key,
            serialized,
            expiry);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T?> GetAsync(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);

            if (value.HasValue)
            {
                _logger.LogDebug("RedisService: Key '{Key}' found", key);
                return JsonSerializer.Deserialize<T>(value!);
            }
            else
            {
                _logger.LogDebug("RedisService: Key '{Key}' not found", key);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RedisService: Error getting key '{Key}'", key);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiry"></param>
    public async Task UpdateExpiryAsync(string key, TimeSpan expiry)
    {
        await _database.KeyExpireAsync(key, expiry);
    }
}