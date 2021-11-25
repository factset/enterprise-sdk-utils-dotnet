using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace FactSet.SDK.Utils.Authentication
{
    /// <summary>
    /// Provides an instance of a validated configuration to be used for creating JWTs. 
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets the Client ID registered with FactSet's Developer Portal.
        /// </summary>
        /// <returns>The Client ID.</returns>
        public string ClientId { get; }
        /// <summary>
        /// Gets the Client Authentication Type.
        /// </summary>
        /// <returns>The Client Authentication Type.</returns>
        public string ClientAuthType { get; }
        /// <summary>
        /// Gets the Well Known URI of the authorization server.
        /// </summary>
        /// <returns>The Well Known URI.</returns>
        public string WellKnownUri { get; } = Constants.FACTSET_WELL_KNOWN_URI;
        /// <summary>
        /// Gets the Json Web Key (JWK).
        /// </summary>
        /// <returns>The JWK.</returns>
        public JsonWebKey Jwk { get; }

        /// <summary>
        /// Creates a valid Configuration instance containing data needed to create a JWT.
        /// </summary>
        /// <param name="clientId">Client ID registered with FactSet's Developer Portal.</param>
        /// <param name="clientAuthType">The client type as defined in OAuth 2.0 in rfc6749 2.1.</param>
        /// <param name="jwk">The JSON Web Key.</param>
        /// <param name="wellKnownUri">Specifies the well-known URI to retrieve metadata about its authorization server.</param>
        /// <exception cref="ArgumentException">Raised if any of the Configuration arguments are null or empty.</exception>
        /// <exception cref="ConfigurationException">Raised if there are any missing essential keys in the JWK as well
        /// as if there are any keys with a value that is null or an empty string.</exception>
        public Configuration(string clientId,
                             string clientAuthType,
                             JsonWebKey jwk,
                             string wellKnownUri = null)
        {
            Trace.TraceInformation("Reviewing configuration format and completeness");

            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException($"'{nameof(clientId)}' cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(clientAuthType))
            {
                throw new ArgumentException($"'{nameof(clientAuthType)}' cannot be null or empty.");
            }

            if (jwk is null)
            {
                throw new ArgumentException($"'{nameof(jwk)}' cannot be null.");
            }

            CheckJwkFormat(jwk);

            ClientId = clientId;
            ClientAuthType = clientAuthType;
            Jwk = jwk;
            WellKnownUri = wellKnownUri ?? WellKnownUri;

            Trace.TraceInformation("Configuration is complete and formatted correctly");
        }

        /// <summary>
        /// A method that takes in the path to a file containing the configuration in a JSON format and converts it to
        /// a Configuration object, if the key value pairs in the JSON are valid (contains required key pairs).
        /// </summary>
        /// <param name="configPath">The file path to the JSON formatted configuration.</param>
        /// <returns>The Configuration object generated.</returns>
        /// <exception cref="ArgumentNullException">Raised if the configPath is null.</exception>
        /// <exception cref="ArgumentException">Raised if any of the Configuration arguments are null or empty.</exception>
        /// <exception cref="IOException">Raised if it fails to read the file specified by the configPath.</exception>
        /// <exception cref="ConfigurationException">Raised if there are any missing essential keys in the JWK as well
        /// as if there are any keys with a value that is null or an empty string.</exception>
        public static Configuration Parse(string configPath)
        {
            if (configPath == null)
            {
                throw new ArgumentNullException(nameof(configPath));
            }

            try
            {
                using (StreamReader streamReader = new StreamReader(configPath))
                {
                    string json = streamReader.ReadToEnd();
                    Configuration configuration = JsonConvert.DeserializeObject<Configuration>(json);

                    Trace.TraceInformation($"Retrieved configuration from file: {configPath}");

                    return configuration;
                }
            }
            catch (Exception e)
            {
                if (e is IOException || e is ConfigurationException || e is ArgumentException)
                {
                    throw;
                }
                throw new ConfigurationException($"Exception caught when retrieving contents of {configPath}", e);
            }
        }

        private void CheckJwkFormat(JsonWebKey jwk)
        {
            if (!IsJwkValid(jwk))
            {
                throw new ConfigurationException($"JWK must contain the following items: " +
                                               $"{string.Join(", ", Constants.CONFIG_JWK_REQUIRED_KEYS)}");
            }
        }

        private bool IsJwkValid(JsonWebKey jwk)
        {
            List<string> propertiesKeys = jwk.GetType().GetProperties()
                .Where(propertyInfo => propertyInfo.GetValue(jwk) != null)
                .Where(propertyInfo => propertyInfo.GetValue(jwk).ToString() != "")
                .Select(propertyInfo => propertyInfo.Name.ToLower()).ToList();

            return Constants.CONFIG_JWK_REQUIRED_KEYS.IsSubsetOf(propertiesKeys);
        }
    }
}
