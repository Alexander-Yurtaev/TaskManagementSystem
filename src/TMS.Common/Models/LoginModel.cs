using System.Text.Json.Serialization;

namespace TMS.Common.Models;

/// <summary>
/// 
/// </summary>
/// <param name="UserName"></param>
/// <param name="Password"></param>
public record LoginModel(string UserName, string Password)
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("username")]
    public string UserName { get; init; } = UserName;

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("password")]
    public string Password { get; init; } = Password;
}