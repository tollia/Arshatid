using System.Net.Http.Headers;

namespace ArshatidPublic.Classes
{
    public class ApiTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiTokenHandler(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var context = _httpContextAccessor.HttpContext;

            // Get the token directly from the Items collection.
            var tokenToUse = context?.Items["jwt_token"] as string;

            if (string.IsNullOrWhiteSpace(tokenToUse))
            {
                // This exception is now correct, as it's the only place we look.
                throw new InvalidOperationException(
                    "No authentication token found in HttpContext.Items.");
            }

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenToUse);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}