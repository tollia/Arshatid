using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ArshatidPublic.Classes
{

    // This is the custom interface we're implementing.
    public interface ITokenHandlerService
    {
        Task<ClaimsPrincipal?> ValidateTokenAsync(string jwt);
    }

    // This is the concrete implementation of the service.
    public class IslandIsTokenHandlerService : ITokenHandlerService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IslandIsTokenHandlerService> _logger;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        public IslandIsTokenHandlerService(IConfiguration configuration, ILogger<IslandIsTokenHandlerService> logger, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;

            // This object handles fetching and caching the OIDC metadata (like signing keys)
            // from the .well-known/openid-configuration endpoint.
            var authority = _configuration["Oidc:Authority"];
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{authority}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever(httpClientFactory.CreateClient()));
        }

        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string jwt)
        {
            // Get the discovery document from the cache or fetch it from the authority.
            var oidcConfig = await _configurationManager.GetConfigurationAsync();

            // These are the same validation parameters from your AddJwtBearer setup.
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = oidcConfig.Issuer,

                ValidateAudience = true,
                ValidAudience = _configuration["Oidc:Audience"],

                ValidateIssuerSigningKey = true,
                // Here, we provide the signing keys we just discovered.
                IssuerSigningKeys = oidcConfig.SigningKeys,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = "name"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Validate the token using the discovered keys and parameters.
                var principal = tokenHandler.ValidateToken(jwt, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "JWT validation failed using IslandIsTokenHandlerService.");
                return null;
            }
        }
    }
}
