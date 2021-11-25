using System.Threading.Tasks;
using FactSet.SDK.Utils.Authentication;
using NUnit.Framework;

namespace FactSet.SDK.Utils.Tests.Authentication
{
    class OAuth2ClientTests
    {
        [Test]
        public void OAuth2ClientGoodInstantiation()
        {
            OAuth2ClientCorrect oAuth = new OAuth2ClientCorrect();
            Assert.IsInstanceOf<OAuth2ClientCorrect>(oAuth);
        }
    }

    public class OAuth2ClientCorrect : IOAuth2Client
    {
        public async Task<string> GetAccessTokenAsync()
        {
            await Task.Delay(1);
            return "string";
        }
    }
}
