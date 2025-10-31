using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Entities.Notification.Enums;
using TMS.Entities.Task;

namespace TMS.Entities.Notification;

[Table("Notification")]
public class NotificationEntity
{
    public int Id { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    [DefaultValue(NotificationReadStatus.Unread)]
    public NotificationReadStatus ReadStatus { get; set; }

    [Required]
    public int TaskId { get; set; }

    public TaskEntity? Task { get; set; }

    [Required]
    public int UserId { get; set; }
}