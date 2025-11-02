using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.TaskService.Entities.Enum;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace TMS.TaskService.Entities;

/// <summary>
/// 
/// </summary>
[Table("Task")]
public class TaskEntity
{
    /// <summary>
    /// 
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public TaskStatus Status { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public TaskPriority Priority { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ProjectEntity? Project { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    public int AssigneeId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<CommentEntity> Comments { get; set; } = [];

    /// <summary>
    /// 
    /// </summary>
    public List<AttachmentEntity> Attachments { get; set; } = [];
}