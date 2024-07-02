using System.Security.Claims;

namespace Chaldea.Fate.RhoAias;

public static class Extensions
{
    public static Guid UserId(this ClaimsPrincipal user)
    {
        var sub = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (sub != null)
        {
            return Guid.Parse(sub.Value);
        }

        return Guid.Empty;
    }
}
