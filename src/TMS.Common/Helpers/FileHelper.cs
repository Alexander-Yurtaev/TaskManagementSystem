using Microsoft.Extensions.Logging;

namespace TMS.Common.Helpers;

public static class FileHelper
{
    public static void ThrowIfPathNotSafe(string basePath, string userPath, ILogger logger)
    {
        if (!IsPathSafe(basePath, userPath))
        {
            logger.LogError("Path traversal attempt detected. Base: {BasePath}, User: {UserPath}",
                                            basePath, userPath);

            throw new InvalidOperationException("Invalid file path: path traversal is not allowed.");
        }
    }

    public static bool IsPathSafe(string basePath, string userPath)
    {
        ArgumentNullException.ThrowIfNull(basePath);

        var fullBasePath = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(basePath, userPath));

        return fullPath.StartsWith(fullBasePath + Path.DirectorySeparatorChar) ||
               fullPath == fullBasePath;
    }
}
