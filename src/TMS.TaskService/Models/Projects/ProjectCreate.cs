using TMS.TaskService.Entities.Enum;

namespace TMS.TaskService.Models.Projects;

/// <summary>
/// 
/// </summary>
public record ProjectCreate : ProjectModelBase
{
    /// <summary>
    /// 
    /// </summary>
    public int UserId { get; set; }
}