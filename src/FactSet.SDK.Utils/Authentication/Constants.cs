using System.Collections.Generic;
using System.Collections.Immutable;

namespace FactSet.SDK.Utils.Authentication
{
    /// <summary>
    /// Contains the constants used by the ConfidentialClient to perform requests for both the metadata of the Well 
    /// Known URI and the access token.
    /// </summary>
    public static class Constants
    {
        // Confidential client assertion JWT
        public static readonly int CC_JWT_NOT_BEFORE_SECS = 5;
        public static readonly int CC_JWT_EXPIRE_AFTER_SECS = 300;

        // Auth server metadata
        public static readonly string META_ISSUER = "issuer";
        public static readonly string META_TOKEN_ENDPOINT = "token_endpoint";

        public static readonly ImmutableHashSet<string> CONFIG_JWK_REQUIRED_KEYS = new HashSet<string> {
            "kty", "alg", "use", "kid", "n", "e", "d", "p", "q", "dp", "dq", "qi"
        }.ToImmutableHashSet();

        // Default values
#pragma warning disable S1075 // URIs should not be hardcoded
        public static readonly string FACTSET_WELL_KNOWN_URI = "https://auth.factset.com/.well-known/openid-configuration";
#pragma warning restore S1075 // URIs should not be hardcoded

    }
}
