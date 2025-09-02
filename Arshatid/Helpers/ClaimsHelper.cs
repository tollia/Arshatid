using System.Security.Claims;

namespace Arshatid.Helpers;

public class ClaimsHelper
{
    public string GetSsn(ClaimsPrincipal user)
    {
        return user.FindFirst("nationalId")?.Value ?? string.Empty;
    }

    public string GetPhone(ClaimsPrincipal user)
    {
        return user.FindFirst("phone_number")?.Value ?? string.Empty;
    }

    public string GetEmail(ClaimsPrincipal user)
    {
        return user.FindFirst("email")?.Value ?? string.Empty;
    }

    public string GetGender(ClaimsPrincipal user)
    {
        return user.FindFirst("gender")?.Value ?? string.Empty;
    }
}
