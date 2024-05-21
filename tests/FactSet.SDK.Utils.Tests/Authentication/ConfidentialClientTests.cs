using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FactSet.SDK.Utils.Authentication;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace FactSet.SDK.Utils.Tests.Authentication
{
    class ConfidentialClientTests
    {
        private HttpClient _testHttpClientEmptyRes;
        private HttpClient _testHttpClientValidRes;
        private string _resourcesPath;
        private static HttpRequestMessage _capturedRequest;

        [SetUp]
        public void Setup()
        {
            _resourcesPath = Path.Join(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.ToString(),
                "Resources");
            _testHttpClientEmptyRes = CreateMockHttp(Path.Join(_resourcesPath, "emptyJson.json"));
            _testHttpClientValidRes = CreateMockHttp(Path.Join(_resourcesPath, "exampleResponseWellKnownUri.txt"));
        }

        [Test]
        public async Task CreateAsync_PassNullPath_ThrowsArgumentNullException()
        {
            try
            {
                await ConfidentialClient.CreateAsync(configPath: null, httpClient: _testHttpClientEmptyRes);
            }
            catch (Exception e)
            {
                Assert.That("Value cannot be null. (Parameter 'configPath')", Is.EqualTo(e.Message));
                Assert.Throws<ArgumentNullException>(() => throw e);
            }
        }

        [Test]
        public async Task CreateAsync_PassingExplicitNull_ThrowsArgumentNullException()
        {
            try
            {
                await ConfidentialClient.CreateAsync(config: null, httpClient: _testHttpClientEmptyRes);
            }
            catch (Exception e)
            {
                Assert.That("Value cannot be null. (Parameter 'config')", Is.EqualTo(e.Message));
                Assert.Throws<ArgumentNullException>(() => throw e);
            }
        }

        [Test]
        public async Task CreateAsync_PassingInvalidConfig_ThrowsArgumentException()
        {
            try
            {
                await ConfidentialClient.CreateAsync(
                    new Configuration("", "", new JsonWebKey()),
                    httpClient: _testHttpClientEmptyRes
                );
            }
            catch (Exception e)
            {
                Assert.That("'clientId' cannot be null or empty.", Is.EqualTo(e.Message));
                Assert.Throws<ArgumentException>(() => throw e);
            }
        }

        [Test]
        public async Task CreateAsync_PassingInvalidConfigPath_ThrowsDirectoryNotFoundException()
        {
            try
            {
                await ConfidentialClient.CreateAsync(
                    "somemoretests.txt",
                    httpClient: _testHttpClientEmptyRes
                );

                // Should fail if no exception is thrown, since that file does not exist.
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.Throws<FileNotFoundException>(() => throw e);
            }
        }

        [Test]
        public async Task CreateAsync_PassingValidConfigPathNonJsonConfig_ThrowsConfigurationException()
        {
            try
            {
                await ConfidentialClient.CreateAsync(
                    Path.Join(_resourcesPath, "badJson.txt"),
                    httpClient: _testHttpClientEmptyRes
                );
            }
            catch (Exception e)
            {
                Assert.That($"Exception caught when retrieving contents of {Path.Join(_resourcesPath, "badJson.txt")}",
                    Is.EqualTo(e.Message));
                Assert.Throws<ConfigurationException>(() => throw e);
            }
        }

        [Test]
        public async Task CreateAsync_PassingValidConfigPathInvalidConfig_ThrowsArgumentException()
        {
            try
            {
                await ConfidentialClient.CreateAsync(
                    Path.Join(_resourcesPath, "invalidConfig.json"),
                    httpClient: _testHttpClientEmptyRes
                );
            }
            catch (Exception e)
            {
                Assert.That($"'clientId' cannot be null or empty.", Is.EqualTo(e.Message));
                Assert.Throws<ArgumentException>(() => throw e);
            }
        }

        [Test]
        public async Task CreateAsync_PassingMissingJwk_ThrowsArgumentException()
        {
            try
            {
                await ConfidentialClient.CreateAsync(
                    Path.Join(_resourcesPath, "missingJwk.txt"),
                    httpClient: _testHttpClientEmptyRes
                );
            }
            catch (Exception e)
            {
                Assert.That($"'jwk' cannot be null.", Is.EqualTo(e.Message));
                Assert.Throws<ArgumentException>(() => throw e);
            }
        }

        [Test]
        public async Task CreateAsync_PassingInvalidJwk_ThrowsConfigurationException()
        {
            try
            {
                await ConfidentialClient.CreateAsync(
                    Path.Join(_resourcesPath, "invalidJwk.txt"),
                    httpClient: _testHttpClientEmptyRes
                );
            }
            catch (Exception e)
            {
                Assert.That(
                    $"JWK must contain the following items: {string.Join(", ", Constants.CONFIG_JWK_REQUIRED_KEYS)}",
                    Is.EqualTo(e.Message));
                Assert.Throws<ConfigurationException>(() => throw e);
            }
        }

        [Test]
        public async Task CreateAsync_PassingInvalidJwkEmptyStringValue_ThrowsConfigurationException()
        {
            try
            {
                await ConfidentialClient.CreateAsync(
                    Path.Join(_resourcesPath, "invalidJwkEmptyStringValue.txt"),
                    httpClient: _testHttpClientEmptyRes
                );
            }
            catch (Exception e)
            {
                Assert.That(
                    $"JWK must contain the following items: {string.Join(", ", Constants.CONFIG_JWK_REQUIRED_KEYS)}",
                    Is.EqualTo(e.Message));
                Assert.Throws<ConfigurationException>(() => throw e);
            }
        }

        [Test]
        public async Task CreateAsync_PassingConfigInvalidJwkEmptyStringValue_ThrowsConfigurationException()
        {
            try
            {
                JsonWebKey jsonWebKey = new JsonWebKey(@"
                    {
                        'kty': '',
                        'use': '',
                        'alg': '',
                        'kid': '',
                        'd': '',
                        'n': '',
                        'e': '',
                        'p': '',
                        'q': '',
                        'dp': '',
                        'dq': '',
                        'qi': ''
                    }");
                await ConfidentialClient.CreateAsync(
                    new Configuration("test", "test", jsonWebKey),
                    httpClient: _testHttpClientEmptyRes
                );
            }
            catch (Exception e)
            {
                Assert.That(
                    $"JWK must contain the following items: {string.Join(", ", Constants.CONFIG_JWK_REQUIRED_KEYS)}",
                    Is.EqualTo(e.Message));
                Assert.Throws<ConfigurationException>(() => throw e);
            }
        }

        [Test]
        public async Task CreateAsync_PassingConfigPathEmptyValues_ThrowsArgumentException()
        {
            try
            {
                await ConfidentialClient.CreateAsync(
                    Path.Join(_resourcesPath, "emptyValuesJson.json"),
                    httpClient: _testHttpClientEmptyRes
                );
            }
            catch (Exception e)
            {
                Assert.That($"'clientId' cannot be null or empty.", Is.EqualTo(e.Message));
                Assert.Throws<ArgumentException>(() => throw e);
            }
        }

        [Test]
        public async Task HttpClient_UserAgent_ContainsCorrectAgent()
        {
            HttpClient mockClient =
                CreateMockHttp(Path.Join(_resourcesPath, "exampleResponseWellKnownUri.txt"));

            ConfidentialClient confidentialClient = await ConfidentialClient.CreateAsync(
                Path.Join(_resourcesPath, "validConfigGeneratedSample.txt"),
                mockClient
            );
            
            Assert.That(_capturedRequest.Headers.UserAgent.ToString(), Is.EqualTo(Constants.USER_AGENT));
        }

        [Test]
        public async Task CreateAsync_PassingInvalidWellKnownUri_ThrowsWellKnownUriException()
        {
            var mockHandlerLocal = new Mock<HttpClientHandler>(MockBehavior.Strict);
            mockHandlerLocal
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .Throws(new Exception());

            var testHttpClient = new HttpClient(mockHandlerLocal.Object);

            try
            {
                await ConfidentialClient.CreateAsync(
                    Path.Join(_resourcesPath, "validConfig.txt"),
                    httpClient: testHttpClient
                );
            }
            catch (Exception e)
            {
                Assert.That($"Error retrieving contents from the well_known_uri: {Constants.FACTSET_WELL_KNOWN_URI}",
                    Is.EqualTo(e.Message));
                Assert.That(e, Is.InstanceOf<WellKnownUriException>());
            }
        }

        [Test]
        public async Task CreateAsync_PassingValidConfigPathValidConfigInvalidUri_ThrowsWellKnownUriContentException()
        {
            try
            {
                await ConfidentialClient.CreateAsync(
                    Path.Join(_resourcesPath, "validConfig.json"),
                    httpClient: _testHttpClientEmptyRes
                );
            }
            catch (Exception e)
            {
                Assert.Throws<WellKnownUriContentException>(() => throw e);
            }
        }

        [Test]
        public async Task CreateAsync_PassingValidConfigPathValidConfigJSON_InitialisesWithNoException()
        {
            try
            {
                ConfidentialClient confidentialClient = await ConfidentialClient.CreateAsync(
                    Path.Join(_resourcesPath, "validConfig.json"),
                    httpClient: _testHttpClientValidRes
                );

                Assert.That(confidentialClient, Is.InstanceOf<ConfidentialClient>());
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [Test]
        public async Task
            CreateAsync_InitialiseAndCastConfidentialClientToIOAuth2Client_InitialisesAndCastsSuccessfully()
        {
            ConfidentialClient confidentialClient = await ConfidentialClient.CreateAsync(
                Path.Join(_resourcesPath, "validConfig.json"),
                httpClient: _testHttpClientValidRes
            );

            Assert.That(confidentialClient, Is.InstanceOf<ConfidentialClient>());
            Assert.That((IOAuth2Client)confidentialClient, Is.InstanceOf<IOAuth2Client>());
        }

        [Test]
        public async Task GetAccessTokenAsync_CallingGetAccessTokenWithFailedSigning_RaisesSigningJwsException()
        {
            int expireTime = 100;
            HttpClient mockClient =
                CreateMockHttpTokenRequest(Path.Join(_resourcesPath, "exampleResponseWellKnownUri.txt"), expireTime);

            // validConfig.txt is a valid config format, but invalid for signing a JWT.
            ConfidentialClient confidentialClient = await ConfidentialClient.CreateAsync(
                Path.Join(_resourcesPath, "validConfig.txt"),
                mockClient
            );

            try
            {
                await confidentialClient.GetAccessTokenAsync();
            }
            catch (Exception e)
            {
                Assert.That($"Failed signing of the JWS", Is.EqualTo(e.Message));
                Assert.Throws<SigningJwsException>(() => throw e);
            }
        }

        [Test]
        public async Task GetAccessTokenAsync_CallingGetAccessTokenWithErrorResponse_RaisesAccessTokenException()
        {
            HttpClient mockClient =
                CreateErroneousMockHttpTokenRequest(Path.Join(_resourcesPath, "exampleResponseWellKnownUri.txt"));

            // validConfig.txt is a valid config format, but invalid for signing a JWT.
            ConfidentialClient confidentialClient = await ConfidentialClient.CreateAsync(
                Path.Join(_resourcesPath, "validConfigGeneratedSample.txt"),
                mockClient
            );

            try
            {
                await confidentialClient.GetAccessTokenAsync();
            }
            catch (Exception e)
            {
                Assert.That($"Error attempting to get access token", Is.EqualTo(e.Message));
                Assert.Throws<AccessTokenException>(() => throw e);
            }
        }

        [Test]
        public async Task GetAccessTokenAsync_CallingGetAccessTokenForTheFirstTime_ReturnsNewAccessToken()
        {
            int expireTime = 100;
            HttpClient mockClient =
                CreateMockHttpTokenRequest(Path.Join(_resourcesPath, "exampleResponseWellKnownUri.txt"), expireTime);

            ConfidentialClient confidentialClient = await ConfidentialClient.CreateAsync(
                Path.Join(_resourcesPath, "validConfigGeneratedSample.txt"),
                mockClient
            );

            string accessToken = await confidentialClient.GetAccessTokenAsync();

            Assert.That("1234", Is.EqualTo(accessToken));
        }

        [Test]
        public async Task GetAccessTokenAsync_CallingGetAccessTokenTwiceBeforeExpiration_ReturnsSameAccessToken()
        {
            int expireTime = 100;
            HttpClient mockClient =
                CreateMockHttpTokenRequest(Path.Join(_resourcesPath, "exampleResponseWellKnownUri.txt"), expireTime);

            ConfidentialClient confidentialClient = await ConfidentialClient.CreateAsync(
                Path.Join(_resourcesPath, "validConfigGeneratedSample.txt"),
                mockClient
            );

            string accessToken = await confidentialClient.GetAccessTokenAsync();

            Assert.That("1234", Is.EqualTo(accessToken));

            // Called immediately after the first `GetAccessToken` call, so its within expiration.
            accessToken = await confidentialClient.GetAccessTokenAsync();

            Assert.That("1234", Is.EqualTo(accessToken));
        }

        [Test]
        public async Task
            GetAccessTokenAsync_CallingGetAccessTokenBeforeAndAfterExpiration_ReturnsDifferentAccessTokens()
        {
            int expireTime = 0;
            HttpClient mockClient =
                CreateMockHttpTokenRequest(Path.Join(_resourcesPath, "exampleResponseWellKnownUri.txt"), expireTime);

            ConfidentialClient confidentialClient = await ConfidentialClient.CreateAsync(
                Path.Join(_resourcesPath, "validConfigGeneratedSample.txt"),
                mockClient
            );

            string accessToken = await confidentialClient.GetAccessTokenAsync();

            Assert.That("1234", Is.EqualTo(accessToken));

            // Called after a zero expiration.
            accessToken = await confidentialClient.GetAccessTokenAsync();

            Assert.That("4321", Is.EqualTo(accessToken));
        }

        [Test]
        public async Task GetAccessTokenAsync_CallingGetAccessTokenWithFailedResponse_ThrowAccessTokenException()
        {
            string wellKnownUriJson = GetJsonFromFile(Path.Join(_resourcesPath, "exampleResponseWellKnownUri.txt"));

            var mockHandler = new Mock<HttpClientHandler>(MockBehavior.Strict);
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .Throws(new Exception());

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(wellKnownUriJson)
                });

            var testHttpClient = new HttpClient(mockHandler.Object);

            try
            {
                ConfidentialClient confidentialClient = await ConfidentialClient.CreateAsync(
                    Path.Join(_resourcesPath, "validConfigGeneratedSample.txt"),
                    httpClient: testHttpClient
                );

                await confidentialClient.GetAccessTokenAsync();
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf<AccessTokenException>());
                Assert.That($"Error attempting to get access token", Is.EqualTo(e.Message));
            }
        }

        // Helper method for creating a mock http with different responses
        private static HttpClient CreateMockHttp(string getAsyncResponseFilePath)
        {
            string json = GetJsonFromFile(getAsyncResponseFilePath);

            var mockHandler = new Mock<HttpClientHandler>(MockBehavior.Strict);
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((r, c) => _capturedRequest = r)
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            return new HttpClient(mockHandler.Object);
        }

        private static HttpClient CreateErroneousMockHttpTokenRequest(string wellKnownUriPath)
        {
            string wellKnownUriJson = GetJsonFromFile(wellKnownUriPath);

            var mockHandler = new Mock<HttpClientHandler>(MockBehavior.Strict);
            mockHandler
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .Throws(new Exception());

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent(wellKnownUriJson)
                });

            return new HttpClient(mockHandler.Object);
        }

        private static HttpClient CreateMockHttpTokenRequest(string wellKnownUriPath, int expireTime)
        {
            string wellKnownUriJson = GetJsonFromFile(wellKnownUriPath);

            var mockHandler = new Mock<HttpClientHandler>(MockBehavior.Strict);
            mockHandler
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        $"{{\"access_token\":\"1234\",\"token_type\":\"Bearer\",\"expires_in\":\"{expireTime}\"}}")
                })
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        $"{{\"access_token\":\"4321\",\"token_type\":\"Bearer\",\"expires_in\":\"{expireTime}\"}}")
                });

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(wellKnownUriJson)
                });

            return new HttpClient(mockHandler.Object);
        }

        private static string GetJsonFromFile(string path)
        {
            string json = "";
            try
            {
                using StreamReader streamReader = new StreamReader(path);
                json = streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(DirectoryNotFoundException))
                {
                    throw new DirectoryNotFoundException();
                }
            }

            return json;
        }
    }
}
