using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.NotificationService.Entities.Enums;

namespace TMS.NotificationService.Entities;

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

    [Required]
    public int UserId { get; set; }
}