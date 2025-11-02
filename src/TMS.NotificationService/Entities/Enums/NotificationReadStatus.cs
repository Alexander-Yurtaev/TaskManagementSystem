using System.ComponentModel;

namespace TMS.NotificationService.Entities.Enums;

public enum NotificationReadStatus
{
    [Description("Не прочитано")]
    Unread = 1,

    [Description("Прочитано")]
    Read = 2,
}