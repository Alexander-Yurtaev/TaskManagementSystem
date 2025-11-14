using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.NotificationService.Entities.Enums;

namespace TMS.NotificationService.Entities;

/// <summary>
/// 
/// </summary>
[Table("Notification")]
public class NotificationEntity
{
    /// <summary>
    /// 
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [DefaultValue(NotificationReadStatus.Unread)]
    public NotificationReadStatus ReadStatus { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    public int TaskId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    public int UserId { get; set; }
}