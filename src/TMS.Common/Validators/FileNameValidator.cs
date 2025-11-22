using TMS.Common.Validators;

namespace TMS.Common.Helpers;

public class FileNameValidator
{
    private static readonly char[] _invalidChars = Path.GetInvalidFileNameChars();
    private static readonly string[] _reservedNames = {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    // Разрешенные расширения (опционально)
    private static readonly string[] _allowedExtensions = {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".jpg", ".jpeg", ".png", ".gif"
    };

    public static ValidationResult ValidateFileName(string fileName, bool checkExtension = true)
    {
        // Базовые проверки
        if (string.IsNullOrWhiteSpace(fileName))
            return ValidationResult.Error("File name cannot be empty");

        if (fileName.Length > 255)
            return ValidationResult.Error("File name is too long (max 255 characters)");

        // Проверка запрещенных символов
        if (fileName.Any(c => _invalidChars.Contains(c)))
            return ValidationResult.Error("File name contains invalid characters");

        // Проверка зарезервированных имен
        string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        if (_reservedNames.Contains(nameWithoutExtension.ToUpperInvariant()))
            return ValidationResult.Error("File name is reserved by system");

        // Проверка конечных символов
        if (fileName.EndsWith(".") || fileName.EndsWith(" "))
            return ValidationResult.Error("File name cannot end with dot or space");

        // Проверка расширения
        if (checkExtension)
        {
            var extensionResult = ValidateFileExtension(fileName);
            if (!extensionResult.IsValid)
                return extensionResult;
        }

        return ValidationResult.Success();
    }

    public static ValidationResult ValidateFileExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrEmpty(extension))
            return ValidationResult.Error("File must have an extension");

        if (extension.Length > 10)
            return ValidationResult.Error("File extension is too long");

        if (_allowedExtensions != null && !_allowedExtensions.Contains(extension.ToLowerInvariant()))
            return ValidationResult.Error($"File extension '{extension}' is not allowed");

        return ValidationResult.Success();
    }
}