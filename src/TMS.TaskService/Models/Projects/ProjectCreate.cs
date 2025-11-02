using TMS.TaskService.Entities.Enum;

namespace TMS.TaskService.Models.Projects;

/// <summary>
/// 
/// </summary>
public record ProjectCreate
{
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public ProjectStatus Status { get; } = ProjectStatus.Draft;

    /// <summary>
    /// 
    /// </summary>
    public int UserId { get; set; }
}