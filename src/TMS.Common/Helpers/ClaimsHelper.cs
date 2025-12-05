using TMS.Common.Enums;

namespace TMS.Common.Helpers;

public static class ClaimsHelper
{
    public static UserRole? GetCurrentUserRole(System.Security.Claims.ClaimsPrincipal user)
    {
        foreach (UserRole userRole in Enum.GetValues(typeof(UserRole)))
        {
            if (user.IsInRole(userRole.ToString()))
            {
                return userRole;
            }
        }

        return null;
    }
}
