using Microsoft.AspNetCore.Http;

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
    public IFormFile File { get; set; } = null!;
}