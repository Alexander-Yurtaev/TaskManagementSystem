namespace TMS.TaskService.Services;

/// <summary>
/// 
/// </summary>
public interface IFileToStorageService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="file"></param>
    /// <param name="httpClientFactory"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> SendFileToStorageService(string filePath, string fileName, IFormFile file, IHttpClientFactory httpClientFactory);
}
