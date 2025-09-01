using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ArshatidPublic.Classes
{
    public class ManualJwtSignInFilter : IAsyncActionFilter
    {
        private readonly ITokenHandlerService _tokenHandler;

        public ManualJwtSignInFilter(ITokenHandlerService tokenHandler)
        {
            _tokenHandler = tokenHandler;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string? jwt = FindJwtInActionArguments(context.ActionArguments);

            if (string.IsNullOrWhiteSpace(jwt))
            {
                context.Result = new BadRequestObjectResult(new { error = "A 'jwt' parameter or property was not found in the request." });
                return;
            }

            ClaimsPrincipal? principal = await _tokenHandler.ValidateTokenAsync(jwt);
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            
            var authProperties = new AuthenticationProperties();
            authProperties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "jwt_token", Value = jwt }
            });

            context.HttpContext.User = principal;
            context.HttpContext.Items["jwt_token"] = jwt;

            await next();
        }

        private static string? FindJwtInActionArguments(IDictionary<string, object?> actionArguments)
        {
            // 1. First, try to find a simple parameter named "jwt".
            if (actionArguments.TryGetValue("jwt", out var jwtValue) && jwtValue is string jwt)
            {
                return jwt;
            }

            // 2. If not found, search inside any complex model objects for a "Jwt" property.
            foreach (var argument in actionArguments.Values)
            {
                if (argument == null) continue;

                var property = argument.GetType().GetProperty("Jwt",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.IgnoreCase);

                if (property != null && property.GetValue(argument) is string propValue)
                {
                    return propValue;
                }
            }

            return null;
        }
    }
}
