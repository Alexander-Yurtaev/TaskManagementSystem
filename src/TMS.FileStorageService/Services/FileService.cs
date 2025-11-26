using TMS.FileStorageService.Models;
using TMS.FileStorageService.Services;

namespace TMS.Common.Services;

/// <summary>
/// 
/// </summary>
public class FileService : IFileService
{
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<FileService> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileStorage"></param>
    /// <param name="logger"></param>
    public FileService(IFileStorage fileStorage, ILogger<FileService> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public async Task<FileUploadResult> UploadFileAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return FileUploadResult.Failure("Файл не предоставлен");

            // Получаем расширение файла из оригинального имени
            var fileExtension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(fileExtension))
                return FileUploadResult.Failure("Не удалось определить тип файла");

            // Используем using для гарантированного освобождения ресурсов
            using (var stream = file.OpenReadStream())
            {
                var userPath = await _fileStorage.SaveFileAsync(stream, "", fileExtension);

                _logger.LogInformation("Файл {FileName} успешно загружен. Оригинальное имя: {OriginalName}",
                    userPath, file.FileName);

                return FileUploadResult.Success(userPath, file.FileName, file.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке файла {FileName}", file?.FileName);
            return FileUploadResult.Failure($"Ошибка загрузки: {ex.Message}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userPath"></param>
    /// <returns></returns>
    public async Task<FileDownloadResult> DownloadFileAsync(string userPath)
    {
        try
        {
            var stream = await _fileStorage.GetFileAsync(userPath);
            return FileDownloadResult.Success(stream, userPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при скачивании файла {FileName}", userPath);
            return FileDownloadResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userPath"></param>
    /// <returns></returns>
    public async Task<bool> RemoveFileAsync(string userPath)
    {
        try
        {
            return await _fileStorage.DeleteFileAsync(userPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении файла {FileName}", userPath);
            return false;
        }
    }
}