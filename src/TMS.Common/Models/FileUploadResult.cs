namespace TMS.Common.Models;

/// <summary>
/// 
/// </summary>
public class FileUploadResult
{
    /// <summary>
    /// 
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string OriginalName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="originalName"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static FileUploadResult Success(string filePath, string originalName, long size)
    {
        return new FileUploadResult
        {
            IsSuccess = true,
            FilePath = filePath,
            OriginalName = originalName,
            Size = size,
            Message = "Файл успешно загружен"
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static FileUploadResult Failure(string message)
    {
        return new FileUploadResult
        {
            IsSuccess = false,
            Message = message
        };
    }
}
