using TMS.Common.Services;
using TMS.NotificationService.Models;

namespace TMS.NotificationService.Services;

/// <summary>
/// 
/// </summary>
public class EmailFileService : IEmailService
{
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<EmailFileService> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileStorage"></param>
    /// <param name="logger"></param>
    public EmailFileService(IFileStorage fileStorage, ILogger<EmailFileService> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string> Send(IEmail email)
    {
        if (email is not EmailFile emailFile)
        {
            throw new ArgumentException($"'{nameof(email)}' parameter must be EmailFile type.");
        }

        try
        {
            // Преобразуем строку Body в поток
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(email.Body);
            writer.Flush();
            stream.Position = 0;

            // Сохраняем файл с расширением .txt
            var fileName = await _fileStorage.SaveFileAsync(stream, ".txt", emailFile.Path);

            _logger.LogInformation("Email body saved to file: {FileName}", fileName);
            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save email body to file.");
            throw;
        }
    }
}
