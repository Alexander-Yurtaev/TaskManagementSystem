using TMS.Common.Extensions;

namespace TMS.Common.Services;

public class FileService : IFileService
{
    private readonly Lock _lock = new Lock();
    private FileServiceOptions _options = new FileServiceOptions();

    public FileService(FileServiceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options = _options;
    }

    public string BasePath => _options.BasePath;

    public void WriteFile(string path, Action<FileStream> action)
    {
        lock (_lock)
        {
            var filePath = Path.Combine(BasePath, path);
            EnsureDirectoryExists(filePath);

            using var stream = new FileStream(filePath, FileMode.Create);
            action(stream);
        }
    }

    public void WriteFile(string path, string content)
    {
        lock (_lock)
        {
            var filePath = Path.Combine(BasePath, path);
            EnsureDirectoryExists(filePath);
            File.WriteAllText(path, content);
        }
    }

    public Stream GetFile(string filePath)
    {
        var fullPath = Path.Combine(BasePath, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Файл не найден: {fullPath}");

        // Открываем поток для чтения. НЕ закрываем!
        return new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,  // Разрешаем параллельное чтение
            bufferSize: 4096,
            useAsync: true);
    }

    private void EnsureDirectoryExists(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}