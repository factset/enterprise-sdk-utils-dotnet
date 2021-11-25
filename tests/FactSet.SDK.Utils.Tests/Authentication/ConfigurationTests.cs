using System;
using System.Collections.Generic;
using System.IO;
using FactSet.SDK.Utils.Authentication;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;

namespace FactSet.SDK.Utils.Tests.Authentication
{
    class ConfigurationTests
    {
        private string _resourcesPath;
        private string _validJwk;
        private string _invalidJwkMissingKty;

        [SetUp]
        public void Setup()
        {
            _resourcesPath = Path.Join(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.ToString(), "Resources");

            _validJwk = @"
                {
                    'kty': 'test',
                    'use': 'test',
                    'alg': 'test',
                    'kid': 'test',
                    'd': 'test',
                    'n': 'test',
                    'e': 'AQAB',
                    'p': 'test',
                    'q': 'test',
                    'dp': 'test',
                    'dq': 'test',
                    'qi': 'test'
                }";

            _invalidJwkMissingKty = @"
                {
                    'use': 'test',
                    'alg': 'test',
                    'kid': 'test',
                    'd': 'test',
                    'n': 'test',
                    'e': 'AQAB',
                    'p': 'test',
                    'q': 'test',
                    'dp': 'test',
                    'dq': 'test',
                    'qi': 'test'
                }";
        }

        [Test]
        public void Configuration_PassingEverythingNull_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Configuration(clientId: null,
                                                                     clientAuthType: null,
                                                                     jwk: null));
        }

        [Test]
        public void Configuration_PassingClientIdNull_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Configuration(clientId: null,
                                                                     clientAuthType: "test",
                                                                     jwk: new JsonWebKey(_validJwk)));
        }

        [Test]
        public void Configuration_PassingClientAuthTypeNull_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Configuration(clientId: "test",
                                                                     clientAuthType: null,
                                                                     jwk: new JsonWebKey(_validJwk)));
        }

        [Test]
        public void Configuration_PassingJwkNull_ThrowsArgumentException()
        {
            JsonWebKey jsonWebKey = null;
            Assert.Throws<ArgumentException>(() => new Configuration(clientId: "test",
                                                                     clientAuthType: "test",
                                                                     jwk: jsonWebKey));
        }

        [Test]
        public void Configuration_PassingEmptyAndNull_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Configuration(clientId: "",
                                                                     clientAuthType: "",
                                                                     jwk: null));
        }

        [Test]
        public void Configuration_PassingClientIdEmpty_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Configuration(clientId: "",
                                                                     clientAuthType: "test",
                                                                     jwk: new JsonWebKey(_validJwk)));
        }

        [Test]
        public void Configuration_PassingClientAuthTypeEmpty_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Configuration(clientId: "test",
                                                                     clientAuthType: "",
                                                                     jwk: new JsonWebKey(_validJwk)));
        }

        [Test]
        public void Configuration_PassingInvalidJwkMissingKty_ThrowsConfigurationException()
        {
            JsonWebKey jsonWebKey = new(_invalidJwkMissingKty);
            Assert.Throws<ConfigurationException>(() => new Configuration(clientId: "test",
                                                                          clientAuthType: "test",
                                                                          jwk: jsonWebKey));
        }

        [Test]
        public void Configuration_PassingMissingJwk_ThrowsConfigurationException()
        {
            JsonWebKey jsonWebKey = new("{}");
            Assert.Throws<ConfigurationException>(() => new Configuration(clientId: "test",
                                                                          clientAuthType: "test",
                                                                          jwk: jsonWebKey));
        }

        [Test]
        public void Configuration_PassingValidConfig_InstantiatesConfiguration()
        {
            JsonWebKey jsonWebKey = new(_validJwk);
            Assert.IsInstanceOf<Configuration>(new Configuration(clientId: "test",
                                                                 clientAuthType: "test",
                                                                 jwk: jsonWebKey));
        }

        [Test]
        public void Parse_PassingNullConfigPath_ThrowsConfigurationException()
        {
            try
            {
                Configuration.Parse(null);
            }
            catch (Exception e)
            {
                Assert.AreEqual("Value cannot be null. (Parameter 'configPath')", e.Message);
                Assert.Throws<ArgumentNullException>(() => throw e);
            }
        }

        [Test]
        public void Parse_PassingNonExistingFilePath_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => Configuration.Parse(Path.Join(_resourcesPath, "somemoretests.txt")));
        }

        [Test]
        public void Parse_PassingConfigFileWithMissingJwkProperties_ThrowsConfigurationException()
        {
            try
            {
                Configuration.Parse(Path.Join(_resourcesPath, "invalidJwk.txt"));
            }
            catch (Exception e)
            {
                Assert.AreEqual($"JWK must contain the following items: {string.Join(", ", Constants.CONFIG_JWK_REQUIRED_KEYS)}", e.Message);
                Assert.Throws<ConfigurationException>(() => throw e);
            }
        }

        [Test]
        public void Parse_PassingConfigFileWithEmptyStringProperties_ThrowsArgumentException()
        {
            try
            {
                Configuration.Parse(Path.Join(_resourcesPath, "emptyValuesJson.json"));
            }
            catch (Exception e)
            {
                Assert.AreEqual($"'clientId' cannot be null or empty.", e.Message);
                Assert.Throws<ArgumentException>(() => throw e);
            }
        }

        [Test]
        public void Parse_PassingCorrectConfigFileFormat_ReturnsConfigurationInstance()
        {
            Configuration config = Configuration.Parse(Path.Join(_resourcesPath, "validConfig.json"));

            Assert.IsInstanceOf<Configuration>(config);

            // Non-Jwk config properties.
            Assert.AreEqual("testClientId", config.ClientId);
            Assert.AreEqual("testClientAuthType", config.ClientAuthType);
            Assert.AreEqual(Constants.FACTSET_WELL_KNOWN_URI, config.WellKnownUri);

            // Jwk properties.
            Assert.AreEqual("testKty", config.Jwk.Kty);
            Assert.AreEqual("testUse", config.Jwk.Use);
            Assert.AreEqual("testAlg", config.Jwk.Alg);
            Assert.AreEqual("testKid", config.Jwk.Kid);
            Assert.AreEqual("testD", config.Jwk.D);
            Assert.AreEqual("testN", config.Jwk.N);
            Assert.AreEqual("AQAB", config.Jwk.E);
            Assert.AreEqual("testP", config.Jwk.P);
            Assert.AreEqual("testQ", config.Jwk.Q);
            Assert.AreEqual("testDP", config.Jwk.DP);
            Assert.AreEqual("testDQ", config.Jwk.DQ);
            Assert.AreEqual("testQI", config.Jwk.QI);
        }
    }
}
