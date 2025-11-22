using Microsoft.AspNetCore.Http;

namespace TMS.Common.Validators;

public class FileSizeValidator
{
    private const long DefaultMaxFileSize = 10 * 1024 * 1024; // 10MB

    public static ValidationResult ValidateFileSize(IFormFile file, long? maxFileSize = DefaultMaxFileSize)
    {
        if (file == null)
            return ValidationResult.Error("File is null");

        if (file.Length == 0)
            return ValidationResult.Error("File is empty");

        if (file.Length > maxFileSize)
        {
            var maxSizeMB = maxFileSize / 1024 / 1024;
            var actualSizeMB = file.Length / 1024 / 1024;
            return ValidationResult.Error($"File size {actualSizeMB}MB exceeds maximum allowed size {maxSizeMB}MB");
        }

        return ValidationResult.Success();
    }
}