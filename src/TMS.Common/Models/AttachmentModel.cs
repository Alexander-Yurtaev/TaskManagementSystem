namespace TMS.Common.Models;

public record AttachmentModel
{
    /// <summary>
    /// 
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}