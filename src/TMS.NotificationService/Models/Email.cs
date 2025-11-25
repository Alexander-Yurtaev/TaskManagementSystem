namespace TMS.NotificationService.Models;

/// <summary>
/// 
/// </summary>
public record Email : IEmail
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    public Email(string body)
    {
        Body = body;
    }

    /// <summary>
    /// 
    /// </summary>
    public string Body { get; init; }
}