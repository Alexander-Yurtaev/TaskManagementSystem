using TMS.FileStorageService.Models;

namespace TMS.FileStorageService.Services;

/// <summary>
/// 
/// </summary>
public interface IFileService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    Task<FileUploadResult> UploadFileAsync(IFormFile file);

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