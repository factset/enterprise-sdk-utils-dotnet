using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;

namespace FactSet.SDK.Utils.Authentication
{
    /// <summary>
    /// Helper class that supports FactSet's implementation of the OAuth 2.0 client credentials flow. This class
    /// provides methods that retrieve an access token that can be used to authenticate against FactSet's APIs. It takes
    /// care of fetching the token, caching it and refreshing it (when expired) as needed.
    /// </summary>
    public class ConfidentialClient : IOAuth2Client
    {
        private DateTime _jwsIssuedAt;
        private readonly Configuration _config;
        private readonly JwtHeader _jwtHeader;
        private readonly Claim[] _claims;
        private readonly JwtSecurityTokenHandler _jwtHandler = new JwtSecurityTokenHandler();
        private Task<TokenResponse> _cachedTokenTask;
        private readonly object _tokenLock = new object();
        private readonly HttpClient _httpClient;
        private readonly DiscoveryCache _discoveryCache;
        private string _tokenEndpoint;
        private string _issuer;

        private ConfidentialClient(string configPath, HttpClient httpClient = null) : this(Configuration.Parse(configPath), httpClient) { }

        private ConfidentialClient(Configuration config, HttpClient httpClient = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _httpClient = InitClient(httpClient);

            _discoveryCache = new DiscoveryCache(
                authority: config.WellKnownUri,
                httpClientFunc: () => _httpClient
            );

            var signingCredentials = new SigningCredentials(_config.Jwk, _config.Jwk.Alg);
            _jwtHeader = new JwtHeader(signingCredentials);
            _claims = new[] {
                new Claim("sub", _config.ClientId),
                new Claim("jti", _config.Jwk.Kid),
            };
        }

        /// <summary>
        /// Creates a new ConfidentialClient. When setting up the OAuth 2.0 client, this constructor reaches out to
        /// FactSet's well-known URI to retrieve metadata about its authorization server. This information along with 
        /// information about the OAuth 2.0 client is stored and used whenever a new access token is fetched.
        /// </summary>
        /// <param name="configPath">Path to credentials configuration file.</param>
        /// <param name="httpClient">The HttpClient used to request the Well Known URI metadata.</param>
        /// <returns>A new instance of the ConfidentialClient as an async Task.</returns>
        /// <exception cref="ArgumentNullException">Raised if the configPath is null.</exception>
        /// <exception cref="ArgumentException">Raised if any of the Configuration arguments are null or empty.</exception>
        /// <exception cref="WellKnownUriException">Raised if the request for the Well Known URI fails.</exception>
        /// <exception cref="WellKnownUriContentException">Raised if the contents of the Well Known URI metadata had
        /// missing issuer or token_endpoint.</exception>
        /// <exception cref="IOException">Raised if it fails to read the file specified by the configPath.</exception>
        /// <exception cref="ConfigurationException">Raised if there are any missing essential keys in the JWK as well
        /// as if there are any keys with a value that is null or an empty string.</exception>
        public static async Task<ConfidentialClient> CreateAsync(string configPath, HttpClient httpClient = null)
        {
            var client = new ConfidentialClient(configPath, httpClient);
            await client.Init();
            return client;
        }

        /// <summary>
        /// Creates a new ConfidentialClient. When setting up the OAuth 2.0 client, this constructor reaches out to
        /// FactSet's well-known URI to retrieve metadata about its authorization server. This information along with 
        /// information about the OAuth 2.0 client is stored and used whenever a new access token is fetched.
        /// </summary>
        /// <param name="config">Configuration instance.</param>
        /// <param name="httpClient">The HttpClient used to request the Well Known URI metadata.</param>
        /// <returns>A new instance of the ConfidentialClient as an async Task.</returns>
        /// <exception cref="ArgumentNullException">Raised if the configPath is null.</exception>
        /// <exception cref="WellKnownUriException">Raised if the request for the Well Known URI fails.</exception>
        /// <exception cref="WellKnownUriContentException">Raised if the contents of the Well Known URI metadata had
        /// missing issuer or token_endpoint.</exception>
        public static async Task<ConfidentialClient> CreateAsync(Configuration config, HttpClient httpClient = null)
        {
            var client = new ConfidentialClient(config, httpClient);
            await client.Init();
            return client;
        }

        /// <summary>
        /// Returns an access token that can be used for authentication. If the cache contains a valid access token,
        /// it's returned. Otherwise, a new access token is retrieved from FactSet's authorization server. The access
        /// token should be used immediately and not stored to avoid any issues with token expiry. The access token is
        /// used in the Authorization header when accessing FactSet's APIs.
        /// <example>
        /// Example: <code>`{"Authorization": "Bearer access-token"}`</code>
        /// </example>
        /// </summary>
        /// <returns>Returns the access token as an async task string.</returns>
        /// <exception cref="AccessTokenException">Errors pertaining to retrieval of token.</exception>
        /// <exception cref="SigningJwsException">Raised if there is an issue with the signing process of the JWS.</exception>
        public async Task<string> GetAccessTokenAsync()
        {
            if (await IsCachedTokenValid())
            {
                var tokenResult = await _cachedTokenTask;
                TimeSpan span = _jwsIssuedAt.AddSeconds(tokenResult.ExpiresIn).Subtract(DateTime.Now);
                Trace.TraceInformation($"Retrieving cached token. Expires in {span.TotalSeconds} seconds.");
                return tokenResult.AccessToken;
            }

            return await FetchAccessTokenAsync();
        }

        private HttpClient InitClient(HttpClient httpClient)
        {
            if (httpClient == null)
            {
                return new HttpClient();
            }
            return httpClient;
        }

        private TokenClient InitTokenClient()
        {
            var clientAssertion = new ClientAssertion
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = GetClientAssertionJws()
            };
            var tokenClientOptions = new TokenClientOptions
            {
                Address = _tokenEndpoint,
                ClientId = _config.ClientId,
                ClientAssertion = clientAssertion
            };

            return new TokenClient(_httpClient, tokenClientOptions);
        }

        private async Task Init()
        {
            try
            {
                Trace.TraceInformation($"Attempting metadata retrieval from WellKnownUri: {_config.WellKnownUri}");

                var result = await _discoveryCache.GetAsync();

                Trace.TraceInformation($"Request from WellKnownUri completed with status: {result.HttpResponse.StatusCode}");
                Trace.TraceInformation($"Response headers from WellKnownUri were {result.HttpResponse.Headers}");

                _issuer = result.Issuer;
                _tokenEndpoint = result.TokenEndpoint;
            }
            catch (Exception e)
            {
                throw new WellKnownUriException($"Error retrieving contents from the well_known_uri: " +
                                                $"{_config.WellKnownUri}", e);
            }

            if (_issuer == null || _tokenEndpoint == null)
            {
                throw new WellKnownUriContentException($"{Constants.META_ISSUER} and {Constants.META_TOKEN_ENDPOINT} " +
                                                       $"are required within contents of well_known_uri: " +
                                                       $"{_config.WellKnownUri}");
            }
            Trace.TraceInformation($"Retrieved issuer: {_issuer} and tokenEndpoint: {_tokenEndpoint} from WellKnownUri");
        }

        private async Task<bool> IsCachedTokenValid()
        {
            if (_cachedTokenTask == null)
            {
                Trace.TraceInformation("Access token cache is empty");
                return false;
            }

            var tokenResponse = await _cachedTokenTask;
            DateTime expireTime = _jwsIssuedAt.AddSeconds(tokenResponse.ExpiresIn);

            if (DateTime.Now < expireTime)
            {
                return true;
            }
            else
            {
                Trace.TraceInformation($"Cached access token has expired at {expireTime}");
                return false;
            }
        }

        private string GetClientAssertionJws()
        {
            _jwsIssuedAt = DateTime.Now;

            try
            {
                var payload = new JwtPayload(
                    _config.ClientId,
                    _issuer,
                    _claims,
                    _jwsIssuedAt.AddSeconds(-Constants.CC_JWT_NOT_BEFORE_SECS),
                    _jwsIssuedAt.AddSeconds(Constants.CC_JWT_EXPIRE_AFTER_SECS),
                    _jwsIssuedAt
                );

                var secToken = new JwtSecurityToken(_jwtHeader, payload);

                return _jwtHandler.WriteToken(secToken);
            }
            catch (Exception)
            {
                throw new SigningJwsException("Failed signing of the JWS");
            }
        }

        private async Task<string> FetchAccessTokenAsync()
        {
            Trace.TraceInformation("Fetching new access token");

            lock (_tokenLock)
            {
                _cachedTokenTask = InitTokenClient().RequestClientCredentialsTokenAsync();
            }

            // Moved await outside lock to reduce time lock is held for.
            var tokenResponse = await _cachedTokenTask;

            if ((int)tokenResponse.HttpStatusCode >= 300 || tokenResponse.HttpStatusCode == 0)
            {
                throw new AccessTokenException("Error attempting to get access token");
            }

            Trace.TraceInformation($"Caching token that expires at {_jwsIssuedAt.AddSeconds(tokenResponse.ExpiresIn)}");

            return tokenResponse.AccessToken;
        }
    }
}
