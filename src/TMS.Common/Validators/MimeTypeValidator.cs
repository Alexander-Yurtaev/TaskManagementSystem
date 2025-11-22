using Microsoft.AspNetCore.Http;

namespace TMS.Common.Validators;

public class MimeTypeValidator
{
    private static readonly Dictionary<string, string[]> _allowedMimeTypes = new()
    {
        [".pdf"] = new[] { "application/pdf" },
        [".doc"] = new[] { "application/msword" },
        [".docx"] = new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        [".xls"] = new[] { "application/vnd.ms-excel" },
        [".xlsx"] = new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        [".jpg"] = new[] { "image/jpeg" },
        [".jpeg"] = new[] { "image/jpeg" },
        [".png"] = new[] { "image/png" },
        [".gif"] = new[] { "image/gif" },
        [".txt"] = new[] { "text/plain" }
    };

    public static ValidationResult ValidateMimeType(IFormFile file, string[]? allowedExtensions = null)
    {
        if (file == null || file.Length == 0)
            return ValidationResult.Error("File is empty");

        // Получаем расширение файла
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        // Проверяем расширение
        if (allowedExtensions != null && !allowedExtensions.Contains(fileExtension))
            return ValidationResult.Error($"File extension '{fileExtension}' is not allowed");

        // Определяем MIME тип по расширению
        if (!_allowedMimeTypes.TryGetValue(fileExtension, out var expectedMimeTypes))
            return ValidationResult.Error($"File type '{fileExtension}' is not supported");

        // Проверяем MIME тип из заголовка файла
        var actualMimeType = file.ContentType.ToLowerInvariant();

        if (!expectedMimeTypes.Contains(actualMimeType))
            return ValidationResult.Error($"MIME type '{actualMimeType}' does not match file extension");

        return ValidationResult.Success();
    }
}