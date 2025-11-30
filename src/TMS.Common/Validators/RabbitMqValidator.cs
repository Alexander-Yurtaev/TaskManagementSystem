using Microsoft.Extensions.Configuration;

namespace TMS.Common.Validators;

public static class RabbitMqValidator
{
    public static ValidationResult RabbitMqConfigurationValidate(string? host, string? user, string? pass)
    {
        if (string.IsNullOrEmpty(host) ||
            string.IsNullOrEmpty(user) ||
            string.IsNullOrEmpty(pass))
        {
            return ValidationResult.Error("RabbitMQ configuration is not properly set up");
        }

        return ValidationResult.Success();
    }
}
