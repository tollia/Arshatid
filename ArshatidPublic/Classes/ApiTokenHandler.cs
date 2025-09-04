using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

namespace ArshatidPublic.Classes
{
    public class ApiTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiTokenHandler> _logger;

        public ApiTokenHandler(IHttpContextAccessor accessor, ILogger<ApiTokenHandler> logger)
        {
            _httpContextAccessor = accessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpContext? context = _httpContextAccessor.HttpContext;

            // Is the current MVC/Razor/Minimal endpoint marked [AllowAnonymous]?
            bool allowsAnonymous =
                context?.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

            // If we have a token, attach it regardless (useful for anonymous pages where a token is present).
            string? token = context?.Items["jwt_token"] as string;
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else if (!allowsAnonymous)
            {
                throw new InvalidOperationException("No authentication token found in HttpContext.Items.");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
