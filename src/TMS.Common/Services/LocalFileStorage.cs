using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TMS.Common.Helpers;

namespace TMS.Common.Services;

/// <summary>
/// 
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly string _uploadsFolder;
    private readonly string[] _allowedExtensions;
    private readonly long _maxFileSize;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IConfiguration configuration, ILogger<LocalFileStorage> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _uploadsFolder = configuration["FileStorage:UploadsFolder"] ?? "/tmp/files";
        _allowedExtensions = configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>()
                           ?? new[] { ".jpg", ".jpeg", ".png", ".pdf", ".txt" };

        if (!long.TryParse(configuration["FileStorage:MaxFileSize"], out _maxFileSize))
        {
            _maxFileSize = 10485760; // значение по умолчанию
        }

        if (!Directory.Exists(_uploadsFolder))
            Directory.CreateDirectory(_uploadsFolder);
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string path, string fileExtension)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("Файловый поток пуст");

        if (fileStream.Length > _maxFileSize)
            throw new InvalidOperationException($"Файл слишком большой. Максимальный размер: {_maxFileSize / 1024 / 1024}MB");

        var normalizedExtension = fileExtension.ToLowerInvariant();
        if (!_allowedExtensions.Contains(normalizedExtension))
            throw new InvalidOperationException($"Недопустимый тип файла. Разрешены: {string.Join(", ", _allowedExtensions)}");

        var fileName = $"{Guid.NewGuid()}{normalizedExtension}";
        path ??= string.Empty;
        var userPath = Path.Combine(path, fileName);
        
        FileHelper.ThrowIfPathNotSafe(_uploadsFolder, userPath, _logger);

        var filePath = Path.Combine(_uploadsFolder, userPath);
        var dir = Path.GetDirectoryName(filePath) ?? throw new ArgumentException(nameof(filePath));
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using (var fileFileStream = new FileStream(filePath, FileMode.Create))
        {
            // Сбрасываем позицию потока на начало на случай, если он уже читался
            if (fileStream.CanSeek)
                fileStream.Position = 0;

            await fileStream.CopyToAsync(fileFileStream);
        }

        return userPath;
    }

    public Task<Stream> GetFileAsync(string userPath)
    {
        FileHelper.ThrowIfPathNotSafe(_uploadsFolder, userPath, _logger);

        var filePath = Path.Combine(_uploadsFolder, userPath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Файл не найден");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream>(stream);
    }

    public Task<bool> DeleteFileAsync(string userPath)
    {
        var filePath = Path.Combine(_uploadsFolder, userPath);
        if (!File.Exists(filePath))
            return Task.FromResult(false);

        File.Delete(filePath);
        return Task.FromResult(true);
    }
}