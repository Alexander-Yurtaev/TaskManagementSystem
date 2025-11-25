namespace TMS.Common.Helpers;

public static class FileHelper
{
    public static bool IsPathSafe(string basePath, string userPath)
    {
        var fullBasePath = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(basePath, userPath));
        return fullPath.StartsWith(fullBasePath + Path.DirectorySeparatorChar);
    }
}
