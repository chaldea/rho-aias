namespace Chaldea.Fate.RhoAias.Acme.LetsEncrypt;

public class RhoAiasLetsEncryptOptions
{
    public int ChallengeRetries { get; set; } = 50;
    public int ChallengeDelay { get; set; } = 5000;
}
