using TMS.Common.Models;

namespace TMS.FileStorageService.Services;

/// <summary>
/// 
/// </summary>
public interface IFileService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="attachment"></param>
    /// <returns></returns>
    Task<FileUploadResult> UploadFileAsync(AttachmentModel attachment);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<FileDownloadResult> DownloadFileAsync(string fileName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<bool> RemoveFileAsync(string fileName);
}