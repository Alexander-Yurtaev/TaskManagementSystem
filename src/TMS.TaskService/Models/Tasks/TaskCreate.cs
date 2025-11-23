namespace TMS.TaskService.Models.Tasks;

/// <summary>
/// 
/// </summary>
public record TaskCreate : TaskModelBase
{
    /// <summary>
    /// 
    /// </summary>
    public int ProjectId { get; set; }
}