namespace TMS.AuthService.Configurations;

/// <summary>
/// 
/// </summary>
public class RedisConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public RedisConfiguration()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public string Host { get; init; } = string.Empty;
    
    /// <summary>
    /// 
    /// </summary>
    public int Port { get; set; } = 6379;
    
    /// <summary>
    /// 
    /// </summary>
    public string Password { get; init; } = string.Empty;
}
