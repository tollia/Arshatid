using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;

namespace ArshatidPublic.Classes
{
    public class ManualJwtSignInFilter : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ITokenHandlerService _tokenHandler;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly ILogger<ManualJwtSignInFilter> _logger;

        // Run very early
        public int Order => int.MinValue + 100;

        private const string JwtItemKey = "jwt_token";
        private const string JwtTempDataKey = "jwt_token";
        private const string PrincipalItemKey = "jwt_principal";

        public ManualJwtSignInFilter(
            ITokenHandlerService tokenHandler,
            ITempDataDictionaryFactory tempDataFactory,
            ILogger<ManualJwtSignInFilter> logger
        )
        {
            _tokenHandler = tokenHandler;
            _tempDataFactory = tempDataFactory;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            HttpContext http = context.HttpContext;
            var tempData = _tempDataFactory.GetTempData(http);

            // 0) Same-request cache already set?
            if (http.Items.TryGetValue(PrincipalItemKey, out var pObj) &&
                pObj is ClaimsPrincipal cachedPrincipal &&
                cachedPrincipal.Identity?.IsAuthenticated == true)
            {
                http.User = cachedPrincipal;
                // keep TempData JWT alive if present
                if (tempData.ContainsKey(JwtTempDataKey)) tempData.Keep(JwtTempDataKey);
                await next();
                return;
            }

            // Start with request-scoped cache…
            string? jwt = http.Items.TryGetValue(JwtItemKey, out var jObj) ? jObj as string : null;

            // 1) TempData bridge (survives redirect). Keep so it isn't consumed.
            if (string.IsNullOrWhiteSpace(jwt) &&
                tempData.TryGetValue(JwtTempDataKey, out var tdVal) &&
                tdVal is string tdJwt && !string.IsNullOrWhiteSpace(tdJwt))
            {
                jwt = tdJwt;
                tempData.Keep(JwtTempDataKey);
            }

            // 2) Normal sources: query -> route -> Authorization header -> action args -> form
            jwt ??= http.Request.Query["jwt"].FirstOrDefault();

            if (jwt is null && context.RouteData.Values.TryGetValue("jwt", out var rv) && rv != null)
                jwt = rv.ToString();

            if (jwt is null)
            {
                string? auth = http.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(auth) &&
                    auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    jwt = auth.Substring("Bearer ".Length).Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(jwt))
                jwt = FindJwtInActionArguments(context.ActionArguments);

            if (string.IsNullOrWhiteSpace(jwt) && http.Request.HasFormContentType)
            {
                var form = await http.Request.ReadFormAsync();
                jwt = form["jwt"].FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(jwt))
            {
                context.Result = new BadRequestObjectResult(new { error = "JWT not found (query/route/header/param/model/form/TempData)." });
                return;
            }

            _logger.LogInformation("Found jwt={jwt}", jwt);
            ClaimsPrincipal? principal = await _tokenHandler.ValidateTokenAsync(jwt);
            if (principal?.Identity?.IsAuthenticated != true)
            {
                _logger.LogInformation("Jwt token failed validation");
                context.Result = new UnauthorizedResult();
                return;
            }

            // Cache for this request
            http.Items[JwtItemKey] = jwt;
            http.Items[PrincipalItemKey] = principal;
            http.User = principal;

            // Bridge across the *next* request(s)
            tempData[JwtTempDataKey] = jwt;   // cookie-backed & encrypted
            tempData.Keep(JwtTempDataKey);    // don't auto-remove on read

            // Optional: also expose for views to easily add to links
            http.Items["jwt_for_links"] = jwt;

            await next();
        }

        private static string? FindJwtInActionArguments(IDictionary<string, object?> args)
        {
            if (args.TryGetValue("jwt", out var jwtVal) && jwtVal is string s && !string.IsNullOrWhiteSpace(s))
                return s;

            foreach (var v in args.Values)
            {
                if (v is null) continue;

                var t = v.GetType();
                var prop =
                    t.GetProperty("Jwt", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase) ??
                    t.GetProperty("JwtToken", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);

                if (prop?.GetValue(v) is string str && !string.IsNullOrWhiteSpace(str))
                    return str;
            }
            return null;
        }
    }
}
