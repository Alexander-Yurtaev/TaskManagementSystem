using System.Text.Json.Serialization;

namespace TMS.AuthService.Models;

/// <summary>
/// 
/// </summary>
/// <param name="Username"></param>
/// <param name="Password"></param>
public record LoginModel(string Username, string Password)
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; init; } = Username;

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("password")]
    public string Password { get; init; } = Password;
}