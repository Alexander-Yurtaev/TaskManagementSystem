using System.Text.Json.Serialization;
using TMS.Entities.Auth.Enum;

namespace TMS.AuthService.Models;

/// <summary>
/// 
/// </summary>
/// <param name="UserName"></param>
/// <param name="Email"></param>
/// <param name="Password"></param>
/// <param name="Role"></param>
public record RegisterModel(string UserName, string Email, string Password, UserRole Role)
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("username")]
    public string UserName { get; init; } = UserName;

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = Email;

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("password")]
    public string Password { get; init; } = Password;

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("role")]
    public UserRole Role { get; init; } = Role;
}