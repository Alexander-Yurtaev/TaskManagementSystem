using TMS.NotificationService.Models;

namespace TMS.NotificationService.Services;

/// <summary>
/// 
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    Task<string> Send(IEmail email);
}
