namespace TMS.FileStorageService.Extensions;

public static class AppExtensions
{
    public static void CreateFilePath(this WebApplication app, ILogger logger)
    {
        // Получаем путь из переменной окружения
        string? filesPath = app.Configuration["FILES_PATH"];

        if (string.IsNullOrEmpty(filesPath))
        {
            logger.LogCritical("FILES_PATH does not defined.");
            throw new InvalidOperationException("FILES_PATH does not defined.");
        }

        // Проверяем и создаём каталог
        try
        {
            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
                logger.LogInformation($"Directory created: {filesPath}");
            }
            else
            {
                logger.LogInformation($"Directory exists: {filesPath}");
            }
        }
        catch (Exception ex)
        {
            var message = $"Error while work with {filesPath}: {ex.Message}";
            logger.LogCritical(ex, message);
            throw new InvalidOperationException(message, ex);
        }
    }
}