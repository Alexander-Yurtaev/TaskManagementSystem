namespace TMS.TaskService.Models.Comments;

/// <summary>
/// 
/// </summary>
public record CommentCreate
{
    /// <summary>
    /// 
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public int UserId { get; set; }
}