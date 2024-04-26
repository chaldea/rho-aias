using MessagePack;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace Chaldea.Fate.RhoAias;

[MessagePackObject]
public class Client
{
    [Key(0)] public Guid Id { get; set; }
    [Key(1)] public string Name { get; set; }
    [Key(2)] public string? Version { get; set; }
    [Key(3)] public string? Token { get; set; }
    [Key(4)] public string? Endpoint { get; set; }
    [Key(5)] public string? ConnectionId { get; set; }
    [Key(6)] public bool Status { get; set; }
    [Key(7)] public ICollection<Proxy>? Proxies { get; set; }

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
		Endpoint = $"{http.RemoteIpAddress}:{http.RemotePort}";
		ConnectionId = context.ConnectionId;
		Status = true;
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
