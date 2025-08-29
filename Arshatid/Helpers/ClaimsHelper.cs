using System.Security.Claims;

namespace Arshatid.Helpers;

public class ClaimsHelper
{
    public string GetSsn(ClaimsPrincipal user)
    {
        return user.FindFirst("nationalId")?.Value ?? string.Empty;
    }
}
