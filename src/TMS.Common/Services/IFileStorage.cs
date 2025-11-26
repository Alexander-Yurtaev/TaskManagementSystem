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
    /// <param name="path"></param>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    Task<string> SaveFileAsync(Stream fileStream, string path, string fileExtension);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    Task<Stream> GetFileAsync(string filePath);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    Task<bool> DeleteFileAsync(string filePath);
}
