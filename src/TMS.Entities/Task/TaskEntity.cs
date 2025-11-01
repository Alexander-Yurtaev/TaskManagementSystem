using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Entities.Notification;
using TMS.Entities.Task.Enum;
using TaskStatus = TMS.Entities.Task.Enum.TaskStatus;

namespace TMS.Entities.Task;

[Table("Task")]
public class TaskEntity
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime? Deadline { get; set; }

    public TaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public int ProjectId { get; set; }

    public ProjectEntity? Project { get; set; }

    [Required]
    public int AssigneeId { get; set; }

    public List<CommentEntity> Comments { get; set; } = [];

    public List<AttachmentEntity> Attachments { get; set; } = [];

    public List<NotificationEntity> Notifications { get; set; } = [];
}