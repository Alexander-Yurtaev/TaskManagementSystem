namespace TMS.Common.Services;

/// <summary>
/// 
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileExtension, string? path = null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<Stream> GetFileAsync(string fileName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<bool> DeleteFileAsync(string fileName);
}
