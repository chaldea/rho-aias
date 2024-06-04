using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Chaldea.Fate.RhoAias;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Version { get; set; }
    public string? Token { get; set; }
    public string? Endpoint { get; set; }
    public string? ConnectionId { get; set; }
    public bool Status { get; set; }
    public ICollection<Proxy>? Proxies { get; set; }

    public void GenTokenKey()
    {
        Token = Regex.Replace(Convert.ToBase64String(Id.ToByteArray()), "[/+=]", "");
    }

    public Result VersionCheck()
    {
        var version = Utilities.GetVersion();
        if (!VersionCheck(version))
        {
            return Result.Error(ErrorCode.InvalidClientVersion.ToError(version, Version));
        }

        return Result.Success();
    }

    public void Update(HubCallerContext context)
    {
        var http = context.Features.Get<IHttpConnectionFeature>();
        Id = context.User?.UserId() ?? Guid.Empty;
        Endpoint = http == null ? "" : $"{http.RemoteIpAddress}:{http.RemotePort}";
        ConnectionId = context.ConnectionId;
        Status = true;
    }

    public void Update(Client register)
    {
        Endpoint = register.Endpoint;
        ConnectionId = register.ConnectionId;
        Status = register.Status;
        Version = register.Version;
    }

    private bool VersionCheck(Version? serverVersion)
    {
        if (serverVersion == null) return false;
        if (string.IsNullOrEmpty(Version)) return false;
        var clientVersion = new Version(Version);
        if (serverVersion.Major != clientVersion.Major) return false;
        if (serverVersion.Minor != clientVersion.Minor) return false;
        return true;
    }
}