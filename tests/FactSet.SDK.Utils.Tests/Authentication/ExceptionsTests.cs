using System.Runtime.Serialization;
using FactSet.SDK.Utils.Authentication;
using NUnit.Framework;

namespace FactSet.SDK.Utils.Tests.Authentication
{
    class ExceptionsTests
    {
        [Test]
        public void AccessTokenException_EmptyArg_InitialisesWithBaseMessage()
        {
            var exception = new AccessTokenException();
            Assert.AreEqual("Exception of type 'FactSet.SDK.Utils.Authentication.AccessTokenException' was thrown.",
                            exception.Message);
        }

        [Test]
        public void AccessTokenException_MessageArg_InitialisesWithCustomMessage()
        {
            var exception = new AccessTokenException("Test AccessTokenException message");
            Assert.AreEqual("Test AccessTokenException message", exception.Message);
        }

        [Test]
        public void AccessTokenException_MessageArgAndInnerException_InitialisesWithCustomMessageAndInnerException()
        {
            var exception1 = new AccessTokenException("parent");
            var exception2 = new AccessTokenException("Test AccessTokenException message", exception1);
            Assert.AreEqual("Test AccessTokenException message", exception2.Message);
            Assert.AreEqual(exception1, exception2.InnerException);
        }

        [Test]
        public void ConfigurationException_EmptyArg_InitialisesWithBaseMessage()
        {
            var exception = new ConfigurationException();
            Assert.AreEqual("Exception of type 'FactSet.SDK.Utils.Authentication.ConfigurationException' was thrown.",
                            exception.Message);
        }

        [Test]
        public void ConfigurationException_MessageArg_InitialisesWithCustomMessage()
        {
            var exception = new ConfigurationException("Test CredentialsException message");
            Assert.AreEqual("Test CredentialsException message", exception.Message);
        }

        [Test]
        public void ConfigurationException_MessageArgAndInnerException_InitialisesWithCustomMessageAndInnerException()
        {
            var exception1 = new ConfigurationException("parent");
            var exception2 = new ConfigurationException("Test CredentialsException message", exception1);
            Assert.AreEqual("Test CredentialsException message", exception2.Message);
            Assert.AreEqual(exception1, exception2.InnerException);
        }

        [Test]
        public void WellKnownUriException_EmptyArg_InitialisesWithBaseMessage()
        {
            var exception = new WellKnownUriException();
            Assert.AreEqual("Exception of type 'FactSet.SDK.Utils.Authentication.WellKnownUriException' was thrown.",
                            exception.Message);
        }

        [Test]
        public void WellKnownUriException_MessageArg_InitialisesWithCustomMessage()
        {
            var exception = new WellKnownUriException("Test WellKnownUriException message");
            Assert.AreEqual("Test WellKnownUriException message", exception.Message);
        }

        [Test]
        public void WellKnownUriException_MessageArgAndInnerException_InitialisesWithCustomMessageAndInnerException()
        {
            var exception1 = new WellKnownUriException("parent");
            var exception2 = new WellKnownUriException("Test WellKnownUriException message", exception1);
            Assert.AreEqual("Test WellKnownUriException message", exception2.Message);
            Assert.AreEqual(exception1, exception2.InnerException);
        }

        [Test]
        public void WellKnownUriContentException_EmptyArg_InitialisesWithBaseMessage()
        {
            var exception = new WellKnownUriContentException();
            Assert.AreEqual("Exception of type 'FactSet.SDK.Utils.Authentication.WellKnownUriContentException' was thrown.",
                            exception.Message);
        }

        [Test]
        public void WellKnownUriContentException_MessageArg_InitialisesWithCustomMessage()
        {
            var exception = new WellKnownUriContentException("Test WellKnownUriContentException message");
            Assert.AreEqual("Test WellKnownUriContentException message", exception.Message);
        }

        [Test]
        public void WellKnownUriContentException_MessageArgAndInnerException_InitialisesWithCustomMessageAndInnerException()
        {
            var exception1 = new WellKnownUriContentException("parent");
            var exception2 = new WellKnownUriContentException("Test WellKnownUriContentException message", exception1);
            Assert.AreEqual("Test WellKnownUriContentException message", exception2.Message);
            Assert.AreEqual(exception1, exception2.InnerException);
        }

        [Test]
        public void SigningJwsException_EmptyArg_InitialisesWithBaseMessage()
        {
            var exception = new SigningJwsException();
            Assert.AreEqual("Exception of type 'FactSet.SDK.Utils.Authentication.SigningJwsException' was thrown.",
                            exception.Message);
        }

        [Test]
        public void SigningJwsException_MessageArg_InitialisesWithCustomMessage()
        {
            var exception = new SigningJwsException("Test SigningJwsException message");
            Assert.AreEqual("Test SigningJwsException message", exception.Message);
        }

        [Test]
        public void SigningJwsException_MessageArgAndInnerException_InitialisesWithCustomMessageAndInnerException()
        {
            var exception1 = new SigningJwsException("parent");
            var exception2 = new SigningJwsException("Test SigningJwsException message", exception1);
            Assert.AreEqual("Test SigningJwsException message", exception2.Message);
            Assert.AreEqual(exception1, exception2.InnerException);
        }
    }
}
