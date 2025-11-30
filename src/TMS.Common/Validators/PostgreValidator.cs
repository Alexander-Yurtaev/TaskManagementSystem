namespace TMS.Common.Validators;

public class PostgreValidator
{
    public static ValidationResult PostgreSqlConfigurationValidate(string? host, string? db, string? user, string? pass)
    {
        if (string.IsNullOrEmpty(host) ||
            string.IsNullOrEmpty(db) ||
            string.IsNullOrEmpty(user) ||
            string.IsNullOrEmpty(pass))
        {
            return ValidationResult.Error("PostgreSQL configuration is not properly set up");
        }

        return ValidationResult.Success();
    }
}