namespace Microsoft.Extensions.Configuration;

internal static class ConfigurationExtensions
{
	public static string GetRhoAiasConnectionString(this IConfiguration configuration)
	{
		var connStr = configuration.GetConnectionString("RhoAias");
		if (string.IsNullOrEmpty(connStr)) connStr = "Data Source=./data/data.db;Cache=Shared;Mode=ReadWriteCreate;";

		return connStr;
	}
}