namespace TMS.AuthService.Configurations;

/// <summary>
/// 
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// 
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string Audience { get; set; } = string.Empty;
}
