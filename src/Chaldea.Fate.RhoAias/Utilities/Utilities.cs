using System.Reflection;

namespace Chaldea.Fate.RhoAias;

public class Utilities
{
    public static Version? GetVersion()
    {
        var assembly = Assembly.GetAssembly(typeof(Utilities));
        var version = assembly?.GetName().Version;
        return version;
    }

    public static string GetVersionName()
    {
        return GetVersion()?.ToString() ?? string.Empty;
    }

    public static string EnsurePath(params string[] paths)
    {
        var path = Path.Combine(paths);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }
}