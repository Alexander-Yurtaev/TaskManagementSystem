using TMS.Common.Validators;
using TMS.TaskService.Models.Tasks;
using TaskStatus = TMS.TaskService.Entities.Enum.TaskStatus;

namespace TMS.TaskService.Extensions.ApiEndpoints.Tasks;

/// <summary>
/// 
/// </summary>
public static class TaskValidator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static ValidationResult ProjectValidate(TaskModelBase task)
    {
        var trimmedTitle = task.Title?.Trim();

        if (string.IsNullOrEmpty(trimmedTitle))
        {
            return ValidationResult.Error("Task Title must be set.");
        }

        if (trimmedTitle.Length < 2)
        {
            return ValidationResult.Error("Task Title must be at least 2 characters long.");
        }

        if (trimmedTitle.Length > 50)
        {
            return ValidationResult.Error("Length of Task Title must be less or equals 50.");
        }

        var trimmedDescription = task.Description?.Trim();

        if (!string.IsNullOrEmpty(trimmedDescription) && trimmedDescription.Length > 500)
        {
            return ValidationResult.Error("Length of Task Description must be less or equals 500.");
        }

        if (!Enum.IsDefined(typeof(TaskStatus), task.Status))
        {
            return ValidationResult.Error($"Task Status '{task.Status}' is not valid. Must be between 1 and 10.");
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
