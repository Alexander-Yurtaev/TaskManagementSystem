using System.Text.Json.Serialization;

namespace TMS.AuthService.Models;

/// <summary>
/// 
/// </summary>
/// <param name="refreshToken"></param>
public class TokenRefreshModel(string refreshToken)
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("refresh-token")]
    public string RefreshToken { get; set; } = refreshToken;
}