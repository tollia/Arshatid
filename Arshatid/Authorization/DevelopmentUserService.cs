using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace Arshatid.Authorization
{
    public class DevelopmentUserService
    {
        private readonly AuthorizationOptions _options;

        public DevelopmentUserService(IOptions<AuthorizationOptions> options)
        {
            _options = options.Value;
        }

        public bool IsDevelopmentUser(ClaimsPrincipal? user)
        {
            if (user == null)
            {
                return false;
            }

            string? email = user.FindFirst(ClaimTypes.Upn)?.Value
                ?? user.FindFirst(ClaimTypes.Email)?.Value
                ?? user.Identity?.Name;

            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            return _options.DevelopmentUsers.Any(u => string.Equals(u, email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
