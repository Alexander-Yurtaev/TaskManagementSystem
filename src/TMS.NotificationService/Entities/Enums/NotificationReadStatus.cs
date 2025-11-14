using System.ComponentModel;

namespace TMS.NotificationService.Entities.Enums;

/// <summary>
/// 
/// </summary>
public enum NotificationReadStatus
{
    /// <summary>
    /// 
    /// </summary>
    [Description("Не прочитано")]
    Unread = 1,

    /// <summary>
    /// 
    /// </summary>
    [Description("Прочитано")]
    Read = 2,
}