using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ArshatidPublic.Classes
{
    public class TokenHandler
    {
        private ILogger<TokenHandler> _logger;
        private IMemoryCache _cache;
        private TokenValidationParameters _tokenValidationParameters;
        private IConfiguration _configuration;

        public TokenHandler(
            IMemoryCache cache,
            ILogger<TokenHandler> logger,
            IConfiguration configuration
        )
        {
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
            string oidcAuthority = configuration["Oidc:Authority"];
            string oidcAudience = configuration["Oidc:Audience"];
            string oidcWellKnownConfigurationURI = $"{oidcAuthority}/.well-known/openid-configuration";
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, // !!! Calculate longer Lifetime and apply check
                ValidIssuer = oidcAuthority,
                ValidAudience = oidcAudience,

                IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                {
                    // Check if the kid exists in the cache
                    if (!_cache.TryGetValue(kid, out List<SecurityKey> signingKeys))
                    {
                        _logger.LogInformation($"Fetching signing keys for kid: {kid}");

                        try
                        {

                            // If not, fetch the OIDC configuration
                            ConfigurationManager<OpenIdConnectConfiguration> configurationManager = new(
                                oidcWellKnownConfigurationURI,
                                new OpenIdConnectConfigurationRetriever(),
                                new HttpDocumentRetriever()
                            );

                            OpenIdConnectConfiguration? oidcConfiguration = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;

                            signingKeys = oidcConfiguration.SigningKeys
                                .Where(key => key.KeyId == kid)
                                .ToList();

                            // Cache the found signingKeys with the respective kid
                            if (signingKeys.Any())
                            {
                                _cache.Set(kid, signingKeys, TimeSpan.FromHours(1)); // Cache for 1 hour
                            }
                            else
                            {
                                _logger.LogWarning($"No signing keys found for kid: {kid}");
                                throw new SecurityTokenInvalidSigningKeyException($"No signing keys found for kid: {kid}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error fetching OIDC signing keys: {ex.Message}");
                            throw new SecurityTokenException("Unable to retrieve signing keys.", ex);
                        }
                    }
                    return signingKeys;
                }
            };
        }

        /// <summary>
        /// Validates the incoming JWT jwt against the configured OIDC authority.
        /// </summary>
        /// <param name="jwt">JWT jwt as a string</param>
        /// <returns>A ClaimsPrincipal representing the validated jwt</returns>
        public ClaimsPrincipal ValidateToken(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                ClaimsPrincipal principal = handler.ValidateToken(jwt, _tokenValidationParameters, out SecurityToken validatedToken);
                _logger.LogInformation("Token successfully validated.");
                return principal;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning($"Token validation failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Extracts all claims from the ClaimsPrincipal principal as a dictionary.
        /// See comment below for claims issued by innskra.island.is.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns>Dictionary of claims</returns>
        public Dictionary<string, string> GetClaimsFromPricipal(ClaimsPrincipal principal)
        {
            IEnumerable<Claim> claims = principal.Claims;
            return claims
                .GroupBy(c => c.Type)
                .ToDictionary(group => group.Key, group => string.Join(", ", group.Select(c => c.Value)));
        }

        /// <summary>
        /// Extracts all claims from the JWT jwt as a dictionary.
        /// See comment below for claims issued by innskra.island.is.
        /// </summary>
        /// <param name="jwt">JWT jwt as a string</param>
        /// <returns>Dictionary of claims</returns>
        public Dictionary<string, string> GetClaimsFromToken(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = handler.ReadJwtToken(jwt);

                return jwtToken.Claims
                    .GroupBy(c => c.Type)
                    .ToDictionary(group => group.Key, group => string.Join(", ", group.Select(c => c.Value)));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting claims: {ex.Message}");
                throw new SecurityTokenException("Invalid jwt format", ex);
            }
        }

        // Claims issued by innskra.island.is
        // -- For direct authentication subjectType=person without the delegationType claim present
        //{
        //  "Type": "subjectType",
        //  "Value": "person"
        //},
        // -- For delegated authentication for leagal entity, details of authenticated party must be extracted from the actor claim.
        //{
        //    "Type": "subjectType",
        //    "Value": "legalEntity"
        //},
        //{
        //    "Type": "delegationType",
        //    "Value": "ProcurationHolder"
        //},
        //{
        //    "Type": "actor",
        //    "Value": "{\"nationalId\":\"2804673409\",\"name\":\"Þorvaldur Sigurður Arnarson\"}"
        //},
        // -- For delegated authentication for leagal guardian minor, details of authenticated party must be extracted from the actor claim.
        //{
        //    "Type": "subjectType",
        //    "Value": "person"
        //},
        //{
        //    "Type": "delegationType",
        //    "Value": "LegalGuardianMinor"
        //},
        //{
        //    "Type": "actor",
        //    "Value": "{\"nationalId\":\"2709892099\",\"name\":\"Sara Björg Kristjánsdóttir\"}"
        //},
        // -- Shared by all.
        //{
        //  "Type": "nationalId",
        //  "Value": "2804673409"
        //},
        //{
        //  "Type": "audkenni_sim_number",
        //  "Value": "692-2400"
        //},
        //{
        //  "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
        //  "Value": "Þorvaldur Sigurður Arnarson"
        //},
        //{
        //  "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
        //  "Value": "Þorvaldur Sigurður"
        //},
        //{
        //  "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname",
        //  "Value": "Arnarson"
        //},
        //{
        //  "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender",
        //  "Value": "male"
        //},
        //{
        //  "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth",
        //  "Value": "1967-04-28"
        //},
        //{
        //  "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
        //  "Value": "thorvaldur.arnarson@gmail.com"
        //},
        //{
        //  "Type": "email_verified",
        //  "Value": "true"
        //},
        //{
        //  "Type": "phone_number",
        //  "Value": "+354-6922400"
        //},
        //{
        //  "Type": "phone_number_verified",
        //  "Value": "true"
        //}
    }
}
