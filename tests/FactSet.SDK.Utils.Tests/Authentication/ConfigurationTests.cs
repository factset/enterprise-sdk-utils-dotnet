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
            Assert.That(new Configuration(clientId: "test", clientAuthType: "test", jwk: jsonWebKey), Is.InstanceOf<Configuration>());
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
                Assert.That("Value cannot be null. (Parameter 'configPath')", Is.EqualTo(e.Message));
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
                Assert.That($"JWK must contain the following items: {string.Join(", ", Constants.CONFIG_JWK_REQUIRED_KEYS)}", Is.EqualTo(e.Message));
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
                Assert.That($"'clientId' cannot be null or empty.", Is.EqualTo(e.Message));
                Assert.Throws<ArgumentException>(() => throw e);
            }
        }
        
        [Test]
        public void Parse_PassingCorrectConfigFileFormat_ReturnsConfigurationInstance()
        {
            Configuration config = Configuration.Parse(Path.Join(_resourcesPath, "validConfig.json"));
        
            Assert.That(config, Is.InstanceOf<Configuration>());
        
            // Non-Jwk config properties.
            Assert.That("testClientId", Is.EqualTo(config.ClientId));
            Assert.That("testClientAuthType", Is.EqualTo(config.ClientAuthType));
            Assert.That(Constants.FACTSET_WELL_KNOWN_URI, Is.EqualTo(config.WellKnownUri));
        
            // Jwk properties.
            Assert.That("testKty", Is.EqualTo(config.Jwk.Kty));
            Assert.That("testUse", Is.EqualTo(config.Jwk.Use));
            Assert.That("testAlg", Is.EqualTo(config.Jwk.Alg));
            Assert.That("testKid", Is.EqualTo(config.Jwk.Kid));
            Assert.That("testD", Is.EqualTo(config.Jwk.D));
            Assert.That("testN", Is.EqualTo(config.Jwk.N));
            Assert.That("AQAB", Is.EqualTo(config.Jwk.E));
            Assert.That("testP", Is.EqualTo(config.Jwk.P));
            Assert.That("testQ", Is.EqualTo(config.Jwk.Q));
            Assert.That("testDP", Is.EqualTo(config.Jwk.DP));
            Assert.That("testDQ", Is.EqualTo(config.Jwk.DQ));
            Assert.That("testQI", Is.EqualTo(config.Jwk.QI));
        }
    }
}
