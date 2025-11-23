using TMS.Common.Validators;
using TMS.TaskService.Entities.Enum;
using TMS.TaskService.Models.Projects;

namespace TMS.TaskService.Extensions.ApiEndpoints.Projects;

/// <summary>
/// 
/// </summary>
public static class ProjectValidator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public static ValidationResult ProjectValidate(ProjectModelBase project)
    {
        var trimmedName = project.Name?.Trim();

        if (string.IsNullOrEmpty(trimmedName))
        {
            return ValidationResult.Error("Project Name must be set.");
        }

        if (trimmedName.Length < 2)
        {
            return ValidationResult.Error("Project Name must be at least 2 characters long.");
        }

        if (trimmedName.Length > 50)
        {
            return ValidationResult.Error("Length of Project Name must be less or equals 50.");
        }

        var trimmedDescription = project.Description?.Trim();

        if (!string.IsNullOrEmpty(trimmedDescription) && trimmedDescription.Length > 500)
        {
            return ValidationResult.Error("Length of Project Description must be less or equals 500.");
        }

        if (!Enum.IsDefined(typeof(ProjectStatus), project.Status))
        {
            return ValidationResult.Error($"Project Status '{project.Status}' is not valid. Must be between 1 and 10.");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static ValidationResult UserIdValidation(int userId)
    {
        if (userId <= 0)
        {
            return ValidationResult.Error("Project UserId must be positive.");
        }

        return ValidationResult.Success();
    }
}
