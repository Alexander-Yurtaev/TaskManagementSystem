using TMS.Common.Validators;

namespace TMS.TaskService.Models.Attachments;

/// <summary>
/// 
/// </summary>
public record AttachmentUploadRequest
{
    /// <summary>
    /// 
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Валидация модели запроса
    /// </summary>
    /// <returns>Коллекция результатов валидации</returns>
    public IEnumerable<ValidationResult> Validate()
    {
        if (string.IsNullOrWhiteSpace(FileName))
            yield return ValidationResult.Error("FileName is required");

        if (File == null || File.Length == 0)
            yield return ValidationResult.Error("File is required");
    }
}
