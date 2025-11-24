using TMS.Common.Extensions;
using TMS.Common.Helpers;

namespace TMS.Common.Services;

public class FileService : IFileService
{
    private FileServiceOptions _options = new FileServiceOptions();

    public FileService(FileServiceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    public string BasePath => _options.BasePath;

    public void WriteFile(string path, Action<FileStream> action)
    {
        var filePath = Path.Combine(BasePath, path);
        EnsureDirectoryExists(filePath);

        using var stream = new FileStream(filePath, FileMode.Create);
            action(stream);
    }

    public void WriteFile(string path, string content)
    {
        var filePath = Path.Combine(BasePath, path);
        if (!FileHelper.IsPathSafe(BasePath, path))
        {
            throw new InvalidOperationException($"'{filePath}' is wrong.");
        }

        EnsureDirectoryExists(filePath);
        File.WriteAllText(filePath, content);
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