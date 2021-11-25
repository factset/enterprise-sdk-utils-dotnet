using System;
using System.Runtime.Serialization;

namespace FactSet.SDK.Utils.Authentication
{
    /// <summary>
    /// The exception thrown when there is a problem with requesting the access token.
    /// </summary>
    [Serializable]
    public class AccessTokenException : Exception
    {
        /// <summary>
        /// Initialises the AccessTokenException.
        /// </summary>
        public AccessTokenException() { }

        /// <summary>
        /// Initialises the AccessTokenException.
        /// </summary>
        /// <param name="message">The custom message when the exception is thrown.</param>
        public AccessTokenException(string message) : base(message) { }

        /// <summary>
        /// Initialises the AccessTokenException.
        /// </summary>
        /// <param name="message">The custom message when the exception is thrown.</param>
        /// <param name="innerException">The passed down innerException.</param>
        public AccessTokenException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initialises the AccessTokenException.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        protected AccessTokenException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// The exception thrown when there are any missing key value pairs in the Configuration or if the values are null
    /// or an empty string value.
    /// </summary>
    [Serializable]
    public class ConfigurationException : Exception
    {
        /// <summary>
        /// Initialises the ConfigurationException.
        /// </summary>
        public ConfigurationException() { }

        /// <summary>
        /// Initialises the ConfigurationException.
        /// </summary>
        /// <param name="message">The custom message wen the exception is thrown.</param>
        public ConfigurationException(string message) : base(message) { }

        /// <summary>
        /// Initialises the ConfigurationException.
        /// </summary>
        /// <param name="message">The custom message when the exception is thrown.</param>
        /// <param name="innerException">The passed down innerException.</param>
        public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initialises the ConfigurationException.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// The exception thrown when there is a problem requesting the Well Known URI metadata.
    /// </summary>
    [Serializable]
    public class WellKnownUriException : Exception
    {
        /// <summary>
        /// Initialises the WellKnownURIException.
        /// </summary>
        public WellKnownUriException() { }

        /// <summary>
        /// Initialises the WellKnownURIException.
        /// </summary>
        /// <param name="message">The custom message wen the exception is thrown.</param>
        public WellKnownUriException(string message) : base(message) { }

        /// <summary>
        /// Initialises the WellKnownURIException.
        /// </summary>
        /// <param name="message">The custom message when the exception is thrown.</param>
        /// <param name="innerException">The passed down innerException.</param>
        public WellKnownUriException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initialises the WellKnownURIException.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        protected WellKnownUriException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// The exception thrown when the metadata requested from the Well Known URI misses either the "issuer" or the
    /// "token_endpoint".
    /// </summary>
    [Serializable]
    public class WellKnownUriContentException : WellKnownUriException
    {
        /// <summary>
        /// Initialises the WellKnownURIContentException
        /// </summary>
        public WellKnownUriContentException() { }

        /// <summary>
        /// Initialises the WellKnownURIContentException.
        /// </summary>
        /// <param name="message">The custom message wen the exception is thrown.</param>
        public WellKnownUriContentException(string message) : base(message) { }

        /// <summary>
        /// Initialises the WellKnownURIContentException.
        /// </summary>
        /// <param name="message">The custom message when the exception is thrown.</param>
        /// <param name="innerException">The passed down innerException.</param>
        public WellKnownUriContentException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initialises the WellKnownURIContentException.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        protected WellKnownUriContentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// The exception thrown when failing the signing process of the JWT.
    /// </summary>
    [Serializable]
    public class SigningJwsException : Exception
    {
        /// <summary>
        /// Initialises the SigningJwsException
        /// </summary>
        public SigningJwsException()
        {
        }

        /// <summary>
        /// Initialises the SigningJwsException.
        /// </summary>
        /// <param name="message">The custom message wen the exception is thrown.</param>
        public SigningJwsException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initialises the SigningJwsException.
        /// </summary>
        /// <param name="message">The custom message when the exception is thrown.</param>
        /// <param name="innerException">The passed down innerException.</param>
        public SigningJwsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initialises the SigningJwsException.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        protected SigningJwsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
