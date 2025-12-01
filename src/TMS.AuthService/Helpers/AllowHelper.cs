using TMS.AuthService.Entities.Enum;

namespace TMS.AuthService.Helpers;

/// <summary>
/// 
/// </summary>
public static class AllowHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="targetRole"></param>
    /// <returns></returns>
    public static bool CanRegister(System.Security.Claims.ClaimsPrincipal user, UserRole targetRole)
    {
        if (!user.IsInRole(nameof(UserRole.Admin)) && targetRole == UserRole.User)
        {
            return false;
        }

        if (!user.IsInRole(nameof(UserRole.SuperAdmin)) && targetRole == UserRole.Admin)
        {
            return false;
        }

        return true;
    }
}