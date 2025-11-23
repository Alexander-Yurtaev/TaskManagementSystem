namespace TMS.Common.Validators;

public static class CommonValidator
{
    public static ValidationResult EntityNotNullValidate(object entity, string entityName)
    {
        if (entity is null)
        {
            return ValidationResult.Error($"{entityName} must be set.");
        }

        return ValidationResult.Success();
    }
}
