namespace Chaldea.Fate.RhoAias.Authentication.JwtBearer;

internal class RhoAiasJwtOptions
{
	public string Secret { get; set; } = "RhoAias_UChNrHmTtdoyrRUOyJoqBtOzBwsqUdmvUTBhZOAl";
	public string Issuer { get; set; } = "RhoAias";
	public string Audience { get; set; } = "RhoAias";
}