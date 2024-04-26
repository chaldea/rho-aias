namespace Chaldea.Fate.RhoAias;

public class RhoAiasServerOptions
{
    public int Bridge { get; set; } = 8024;
    public int Http { get; set; } = 80;
    public int Https { get; set; } = 443;
    public bool EnableNotification { get; set; } = false;
}
