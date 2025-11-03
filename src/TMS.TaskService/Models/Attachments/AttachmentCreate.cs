namespace TMS.TaskService.Models.Attachments;

/// <summary>
/// 
/// </summary>
public record AttachmentCreate
{
    /// <summary>
    /// 
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
}