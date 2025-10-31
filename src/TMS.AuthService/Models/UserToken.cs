using System.Text.Json.Serialization;

namespace TMS.AuthService.Models;

/// <summary>
/// 
/// </summary>
/// <param name="UserId"></param>
/// <param name="Token"></param>
/// <param name="Expiry"></param>
public record UserToken(int UserId, string Token, DateTime Expiry)
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("user-id")]
    public int UserId { get; init; } = UserId;

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; init; } = Token;

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; init; } = Expiry;
}
