namespace TMS.TaskService.Services;

/// <summary>
/// 
/// </summary>
public class FileToStorageService : IFileToStorageService
{
    private const string FileStorageClientName = "TMS.FileStorageClient";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="file"></param>
    /// <param name="httpClientFactory"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> SendFileToStorageService(
            string filePath,
            string fileName,
            IFormFile file,
            IHttpClientFactory httpClientFactory)
    {
        using var client = httpClientFactory.CreateClient(FileStorageClientName);
        using var content = new MultipartFormDataContent();

        // 1. Добавляем метаданные
        content.Add(new StringContent(fileName), "FileName");
        content.Add(new StringContent(filePath), "FilePath");

        // 2. Добавляем файл
        await using var fileStream = file.OpenReadStream();
        content.Add(new StreamContent(fileStream), "File", file.FileName);

        // 3. Отправляем запрос
        var response = await client.PostAsync("", content);

        return response;
    }
}
