using System.ComponentModel.DataAnnotations;
using TMS.TaskService.Entities.Enum;
using TaskStatus = TMS.TaskService.Entities.Enum.TaskStatus;

namespace TMS.TaskService.Models.Tasks;

/// <summary>
/// 
/// </summary>
public record TaskCreate
{
    /// <summary>
    /// 
    /// </summary>
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
    public TaskStatus Status { get; } = TaskStatus.New;

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
    [Required]
    public int AssigneeId { get; set; }
}