using System.Reflection;

namespace Chaldea.Fate.RhoAias;

internal class Utilities
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
}