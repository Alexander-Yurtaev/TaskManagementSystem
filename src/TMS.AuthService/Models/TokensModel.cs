using System.Text.Json.Serialization;

namespace TMS.AuthService.Models;

/// <summary>
/// 
/// </summary>
/// <param name="accessToken"></param>
/// <param name="refreshToken"></param>
public class TokensModel(string accessToken, string refreshToken)
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = accessToken;

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = refreshToken;
}