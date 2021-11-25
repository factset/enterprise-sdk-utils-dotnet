using System.Threading.Tasks;

namespace FactSet.SDK.Utils.Authentication
{
    /// <summary>
    /// Interface for the OAuth2 code flows Confidential Client and Authorization Code.
    /// </summary>
    public interface IOAuth2Client
    {
        /// <summary>
        /// A template method for requesting an access token.
        /// </summary>
        /// <returns> The access token for protected resource requests</returns>
        Task<string> GetAccessTokenAsync();
    }
}
