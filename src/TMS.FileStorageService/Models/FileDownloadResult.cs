namespace TMS.FileStorageService.Models;

/// <summary>
/// 
/// </summary>
public class FileDownloadResult
{
    /// <summary>
    /// 
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Stream Stream { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static FileDownloadResult Success(Stream stream, string fileName)
    {
        return new FileDownloadResult
        {
            IsSuccess = true,
            Stream = stream,
            FileName = fileName
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static FileDownloadResult Failure(string message)
    {
        return new FileDownloadResult
        {
            IsSuccess = false,
            Message = message
        };
    }
}