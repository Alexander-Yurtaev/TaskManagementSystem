using System.ComponentModel;

namespace TMS.Entities.Notification.Enums;

public enum NotificationReadStatus
{
    [Description("Не прочитано")]
    Unread = 1,

    [Description("Прочитано")]
    Read = 2,
}