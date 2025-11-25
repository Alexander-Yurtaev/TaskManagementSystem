namespace TMS.NotificationService.Models;

/// <summary>
/// 
/// </summary>
public record EmailFile : Email
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    /// <param name="path"></param>
    public EmailFile(string body, string path) : base(body)
    {
        Path = path;
    }

    /// <summary>
    /// 
    /// </summary>
    public string Path { get; init; }

}